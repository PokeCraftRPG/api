using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class AbilityQuerier : IAbilityQuerier
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly DbSet<AbilityEntity> _abilities;
  private readonly ISqlHelper _sqlHelper;

  public AbilityQuerier(IActorService actorService, IContext context, PokemonContext pokemon, ISqlHelper sqlHelper)
  {
    _actorService = actorService;
    _context = context;
    _abilities = pokemon.Abilities;
    _sqlHelper = sqlHelper;
  }

  public async Task<AbilityModel> FindAsync(Ability ability, CancellationToken cancellationToken)
  {
    return await FindAsync(ability.Id, cancellationToken);
  }
  public async Task<AbilityModel> FindAsync(AbilityId id, CancellationToken cancellationToken)
  {
    AbilityEntity ability = await _abilities.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The ability entity 'StreamId={id}' was not found.");
    return await MapAsync(ability, cancellationToken);
  }

  public async Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.EntityId == id && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  public async Task<AbilityModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }

  public async Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Abilities.Table).SelectAll(Db.Abilities.Table)
      .ApplyWorldFilter(Db.Abilities.WorldId, _context.WorldId)
      .ApplyIdFilter(Db.Abilities.EntityId, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Abilities.Key, Db.Abilities.Name);

    IQueryable<AbilityEntity> query = _abilities.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<AbilityEntity>? ordered = null;
    foreach (AbilitySortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case AbilitySort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case AbilitySort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case AbilitySort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case AbilitySort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    AbilityEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<AbilityModel> abilities = await MapAsync(entities, cancellationToken);

    return new SearchResults<AbilityModel>(abilities, total);
  }

  public async Task<AbilityId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _abilities
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new AbilityId(streamId);
  }

  private async Task<AbilityModel> MapAsync(AbilityEntity ability, CancellationToken cancellationToken)
  {
    return (await MapAsync([ability], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<AbilityModel>> MapAsync(IEnumerable<AbilityEntity> abilities, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = abilities.SelectMany(ability => ability.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actorService.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return abilities.Select(mapper.ToAbility).ToList().AsReadOnly();
  }
}
