using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class SpeciesQuerier : ISpeciesQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<RegionalNumberEntity> _regionalNumbers;
  private readonly DbSet<SpeciesEntity> _species;
  private readonly ISqlHelper _sql;

  public SpeciesQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _regionalNumbers = pokemon.RegionalNumbers;
    _species = pokemon.Species;
    _sql = sql;
  }

  public async Task EnsureUnicityAsync(SpeciesAggregate species, CancellationToken cancellationToken)
  {
    Slug? key = null;
    Number? number = null;
    Dictionary<RegionId, Number> regionalNumbers = new(capacity: species.RegionalNumbers.Count);

    foreach (IEvent change in species.Changes)
    {
      if (change is SpeciesCreated created)
      {
        key = created.Key;
        number = created.Number;
      }
      else if (change is SpeciesKeyChanged changed)
      {
        key = changed.Key;
      }
      else if (change is SpeciesRegionalNumberChanged regionalNumber && regionalNumber.Number is not null)
      {
        regionalNumbers[regionalNumber.RegionId] = regionalNumber.Number;
      }
    }

    if (key is not null)
    {
      string? streamId = await _species.Where(x => x.World!.Id == species.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new PropertyConflictException<string>(species, new SpeciesId(streamId).EntityId, key.Value, nameof(SpeciesAggregate.Key));
      }
    }

    if (number is not null)
    {
      string? streamId = await _species.Where(x => x.World!.Id == species.WorldId.ToGuid() && x.Number == number.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new PropertyConflictException<int>(species, new SpeciesId(streamId).EntityId, number.Value, nameof(SpeciesAggregate.Number));
      }
    }

    foreach (KeyValuePair<RegionId, Number> regionalNumber in regionalNumbers)
    {
      string? streamId = await _regionalNumbers.Where(x => x.Region!.StreamId == regionalNumber.Key.Value && x.Number == regionalNumber.Value.Value)
        .Select(x => x.Species!.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new RegionalNumberConflictException(species, new SpeciesId(streamId), regionalNumber.Key, regionalNumber.Value, nameof(SpeciesAggregate.RegionalNumbers));
      }
    }
  }

  public async Task<SpeciesId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string? streamId = await _species.Where(x => x.World!.Id == _context.WorldUid && x.Key == Slug.Normalize(key))
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }

  public async Task<SpeciesModel> ReadAsync(SpeciesAggregate species, CancellationToken cancellationToken)
  {
    return await ReadAsync(species.Id, cancellationToken) ?? throw new InvalidOperationException($"The species entity '{species}' was not found.");
  }
  public async Task<SpeciesModel?> ReadAsync(SpeciesId id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Number == number && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }

  public async Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Species.Table).SelectAll(PokemonDb.Species.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Species.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Species.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Species.Key, PokemonDb.Species.Name);

    if (payload.Category.HasValue)
    {
      builder.Where(PokemonDb.Species.Category, Operators.IsEqualTo(payload.Category.Value.ToString()));
    }
    if (payload.EggGroup.HasValue)
    {
      ComparisonOperator @operator = Operators.IsEqualTo(payload.EggGroup.Value.ToString());
      builder.WhereOr(
        new OperatorCondition(PokemonDb.Species.PrimaryEggGroup, @operator),
        new OperatorCondition(PokemonDb.Species.SecondaryEggGroup, @operator));
    }
    if (payload.GrowthRate.HasValue)
    {
      builder.Where(PokemonDb.Species.GrowthRate, Operators.IsEqualTo(payload.GrowthRate.Value.ToString()));
    }
    if (payload.RegionId.HasValue)
    {
      OperatorCondition condition = new(PokemonDb.Regions.Id, Operators.IsEqualTo(payload.RegionId.Value));
      builder.Join(PokemonDb.RegionalNumbers.SpeciesId, PokemonDb.Species.SpeciesId)
        .Join(PokemonDb.Regions.RegionId, PokemonDb.RegionalNumbers.RegionId, condition);
    }

    IQueryable<SpeciesEntity> query = _species.FromQuery(builder).AsNoTracking()
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region);

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<SpeciesEntity>? ordered = null;
    foreach (SpeciesSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case SpeciesSort.BaseFriendship:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.BaseFriendship) : query.OrderBy(x => x.BaseFriendship))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.BaseFriendship) : ordered.ThenBy(x => x.BaseFriendship));
          break;
        case SpeciesSort.CatchRate:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CatchRate) : query.OrderBy(x => x.CatchRate))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CatchRate) : ordered.ThenBy(x => x.CatchRate));
          break;
        case SpeciesSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case SpeciesSort.EggCycles:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.EggCycles) : query.OrderBy(x => x.EggCycles))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.EggCycles) : ordered.ThenBy(x => x.EggCycles));
          break;
        case SpeciesSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case SpeciesSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case SpeciesSort.Number:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Number) : query.OrderBy(x => x.Number))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Number) : ordered.ThenBy(x => x.Number));
          break;
        case SpeciesSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    SpeciesEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<SpeciesModel> species = await MapAsync(entities, cancellationToken);

    return new SearchResults<SpeciesModel>(species, total);
  }

  private async Task<SpeciesModel> MapAsync(SpeciesEntity species, CancellationToken cancellationToken)
  {
    return (await MapAsync([species], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<SpeciesModel>> MapAsync(IEnumerable<SpeciesEntity> species, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = species.SelectMany(s => s.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return species.Select(mapper.ToSpecies).ToList().AsReadOnly();
  }
}
