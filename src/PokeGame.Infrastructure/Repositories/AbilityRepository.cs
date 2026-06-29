using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Abilities.Models;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Repositories;

internal class AbilityRepository : Repository, IAbilityRepository
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;

  public AbilityRepository(IActorService actorService, IContext context, PokemonContext database, ISqlHelper sqlHelper) : base(database)
  {
    _actorService = actorService;
    _context = context;
    _sqlHelper = sqlHelper;
  }

  public void Add(params Ability[] abilities)
  {
    foreach (Ability ability in abilities)
    {
      Database.Abilities.Add(ability);
      base.RecordChange(new AbilityCreated(ability));
    }
  }
  public void Remove(Ability ability)
  {
    Database.Abilities.Remove(ability);
    base.RecordChange(new AbilityDeleted(ability, _context.UserId));
  }
  public void Update(Ability ability, AbilityUpdated record)
  {
    Database.Abilities.Update(ability);
    base.RecordChange(record);
  }

  public async Task EnsureUnicityAsync(Ability ability, CancellationToken cancellationToken)
  {
    Guid? abilityId = await Database.Abilities.Where(x => x.Key == ability.Key && x.WorldId == _context.WorldId)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (abilityId.HasValue && !abilityId.Value.Equals(ability.Id))
    {
      throw new KeyAlreadyUsedException(ability, abilityId.Value, ability.Key, nameof(Ability.Key));
    }
  }

  public async Task<Ability?> LoadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await Database.Abilities.SingleOrDefaultAsync(x => x.Id == id && x.WorldId == _context.WorldId, cancellationToken);
  }

  public async Task<AbilityModel> ReadAsync(Ability ability, CancellationToken cancellationToken)
  {
    return await ReadAsync(ability.Id, cancellationToken) ?? throw new InvalidOperationException($"The ability 'Id={ability.Id}' was not found.");
  }
  public async Task<AbilityModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    Ability? ability = await Database.Abilities.AsNoTracking()
      .Where(x => x.Id == id && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }
  public async Task<AbilityModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    Ability? ability = await Database.Abilities.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return ability is null ? null : await MapAsync(ability, cancellationToken);
  }

  public async Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Abilities.Table).SelectAll(Db.Abilities.Table)
      .Where(Db.Abilities.WorldId, Operators.IsEqualTo(_context.WorldId))
      .ApplyIdFilter(Db.Abilities.Id, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Abilities.Name);

    IQueryable<Ability> query = Database.Abilities.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<Ability>? ordered = null;
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

    Ability[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<AbilityModel> abilities = await MapAsync(entities, cancellationToken);

    return new SearchResults<AbilityModel>(abilities, total);
  }

  private async Task<AbilityModel> MapAsync(Ability ability, CancellationToken cancellationToken)
  {
    return (await MapAsync([ability], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<AbilityModel>> MapAsync(IEnumerable<Ability> abilities, CancellationToken cancellationToken)
  {
    IEnumerable<Guid> userIds = abilities.SelectMany(ability => ability.GetUserIds());
    IReadOnlyDictionary<Guid, Actor> actors = await _actorService.FindAsync(userIds, cancellationToken);
    Mapper mapper = new(actors);

    return abilities.Select(mapper.ToAbility).ToList().AsReadOnly();
  }
}
