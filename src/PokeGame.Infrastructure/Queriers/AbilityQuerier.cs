using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class AbilityQuerier : IAbilityQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<AbilityEntity> _abilities;

  public AbilityQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _abilities = pokemon.Abilities;
  }

  public async Task<AbilityId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _abilities
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new AbilityId(streamId);
  }

  public async Task<AbilityDto> ReadAsync(Ability ability, CancellationToken cancellationToken)
  {
    return await ReadAsync(ability.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The ability entity 'StreamId={ability.Id}' was not found.");
  }
  public async Task<AbilityDto?> ReadAsync(AbilityId id, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  public async Task<AbilityDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  public async Task<AbilityDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _abilities.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .SingleOrDefaultAsync(cancellationToken);
    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }

  public async Task<SearchResults<AbilityDto>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<AbilityEntity> query = _abilities.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => ability
        => EF.Functions.ILike(ability.Key, pattern, @"\")
        || EF.Functions.ILike(ability.Name!, pattern, @"\")
        || EF.Functions.ILike(ability.Summary!, pattern, @"\"));

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<AbilityDto>(total);
    }

    IOrderedQueryable<AbilityEntity>? ordered = null;
    foreach (SortOption<AbilitySort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case AbilitySort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case AbilitySort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case AbilitySort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case AbilitySort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.AbilityId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    AbilityEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<AbilityDto> abilities = await MapAsync(entities, cancellationToken);

    return new SearchResults<AbilityDto>(abilities, total);
  }

  private async Task<AbilityDto> MapAsync(AbilityEntity ability, CancellationToken cancellationToken)
  {
    return (await MapAsync([ability], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<AbilityDto>> MapAsync(IEnumerable<AbilityEntity> abilities, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = abilities.SelectMany(ability => ability.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return abilities.Select(mapper.ToAbility).ToList().AsReadOnly();
  }
}
