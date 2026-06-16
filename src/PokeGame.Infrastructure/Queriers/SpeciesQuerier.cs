using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class SpeciesQuerier : ISpeciesQuerier
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly DbSet<RegionalNumberEntity> _regionalNumbers;
  private readonly DbSet<SpeciesEntity> _species;
  private readonly ISqlHelper _sqlHelper;

  public SpeciesQuerier(IActorService actorService, IContext context, PokemonContext pokemon, ISqlHelper sqlHelper)
  {
    _actorService = actorService;
    _context = context;
    _regionalNumbers = pokemon.RegionalNumbers;
    _species = pokemon.Species;
    _sqlHelper = sqlHelper;
  }

  public async Task<SpeciesModel> FindAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    return await FindAsync(species.Id, cancellationToken);
  }
  public async Task<SpeciesModel> FindAsync(SpeciesId id, CancellationToken cancellationToken)
  {
    SpeciesEntity species = await _species.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.StreamId == _context.WorldId.Value)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The species entity 'StreamId={id}' was not found.");
    return await MapAsync(species, cancellationToken);
  }

  public async Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.EntityId == id && x.World!.StreamId == _context.WorldId.Value)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Number == number && x.World!.StreamId == _context.WorldId.Value)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.StreamId == _context.WorldId.Value)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }

  public async Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Species.Table).SelectAll(Db.Species.Table)
      .ApplyWorldFilter(Db.Species.WorldId, _context.WorldId)
      .ApplyIdFilter(Db.Species.EntityId, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Species.Key, Db.Species.Name);

    if (payload.Category.HasValue)
    {
      builder.Where(Db.Species.Category, Operators.IsEqualTo(payload.Category.Value.ToString()));
    }
    if (payload.GrowthRate.HasValue)
    {
      builder.Where(Db.Species.GrowthRate, Operators.IsEqualTo(payload.GrowthRate.Value.ToString()));
    }
    if (payload.EggGroup.HasValue)
    {
      builder.WhereOr(
        new OperatorCondition(Db.Species.PrimaryEggGroup, Operators.IsEqualTo(payload.EggGroup.Value.ToString())),
        new OperatorCondition(Db.Species.SecondaryEggGroup, Operators.IsEqualTo(payload.EggGroup.Value.ToString())));
    }
    if (payload.RegionId.HasValue)
    {
      OperatorCondition condition = new(Db.Regions.EntityId, Operators.IsEqualTo(payload.RegionId.Value));
      builder.Join(Db.RegionalNumbers.SpeciesId, Db.Species.SpeciesId)
        .Join(Db.Regions.RegionId, Db.RegionalNumbers.RegionId, condition);
    }

    IQueryable<SpeciesEntity> query = _species.FromQuery(builder).AsNoTracking();

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
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
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

  public async Task<SpeciesId?> TryGetIdAsync(Number number, RegionId? regionId = null, CancellationToken cancellationToken = default)
  {
    if (regionId is null)
    {
      string? streamId = await _species
        .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Number == number.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      return streamId is null ? null : new SpeciesId(streamId);
    }

    string? regionalStreamId = await _regionalNumbers
      .Where(x => x.Region!.StreamId == regionId.Value.Value && x.Number == number.Value && x.Species!.World!.StreamId == _context.WorldId.Value)
      .Select(x => x.Species!.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return regionalStreamId is null ? null : new SpeciesId(regionalStreamId);
  }
  public async Task<SpeciesId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _species
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }

  private async Task<SpeciesModel> MapAsync(SpeciesEntity species, CancellationToken cancellationToken)
  {
    return (await MapAsync([species], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<SpeciesModel>> MapAsync(IEnumerable<SpeciesEntity> species, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = species.SelectMany(entity => entity.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actorService.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return species.Select(mapper.ToSpecies).ToList().AsReadOnly();
  }
}
