using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Repositories;

internal class RegionRepository : Repository, IRegionRepository
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;

  public RegionRepository(IActorService actorService, IContext context, PokemonContext database, ISqlHelper sqlHelper) : base(database)
  {
    _actorService = actorService;
    _context = context;
    _sqlHelper = sqlHelper;
  }

  public void Add(params Region[] regions)
  {
    foreach (Region region in regions)
    {
      Database.Regions.Add(region);
      base.RecordChange(new RegionCreated(region));
    }
  }
  public void Remove(Region region)
  {
    Database.Regions.Remove(region);
    base.RecordChange(new RegionDeleted(region, _context.UserId));
  }
  public void Update(Region region, RegionUpdated record)
  {
    Database.Regions.Update(region);
    base.RecordChange(record);
  }

  public async Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken)
  {
    Guid? regionId = await Database.Regions.Where(x => x.Key == region.Key && x.WorldId == _context.WorldId)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (regionId.HasValue && !regionId.Value.Equals(region.Id))
    {
      throw new KeyAlreadyUsedException(region, regionId.Value, region.Key, nameof(Region.Key));
    }
  }

  public async Task<Region?> LoadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await Database.Regions.SingleOrDefaultAsync(x => x.Id == id && x.WorldId == _context.WorldId, cancellationToken);
  }

  public async Task<RegionModel> ReadAsync(Region region, CancellationToken cancellationToken)
  {
    return await ReadAsync(region.Id, cancellationToken) ?? throw new InvalidOperationException($"The region 'Id={region.Id}' was not found.");
  }
  public async Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    Region? region = await Database.Regions.AsNoTracking()
      .Where(x => x.Id == id && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return region is null ? null : await MapAsync(region, cancellationToken);
  }
  public async Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    Region? region = await Database.Regions.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return region is null ? null : await MapAsync(region, cancellationToken);
  }

  public async Task<SearchResults<RegionModel>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Regions.Table).SelectAll(Db.Regions.Table)
      .Where(Db.Regions.WorldId, Operators.IsEqualTo(_context.WorldId))
      .ApplyIdFilter(Db.Regions.Id, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Regions.Name);

    IQueryable<Region> query = Database.Regions.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<Region>? ordered = null;
    foreach (RegionSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case RegionSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case RegionSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case RegionSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case RegionSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    Region[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<RegionModel> regions = await MapAsync(entities, cancellationToken);

    return new SearchResults<RegionModel>(regions, total);
  }

  private async Task<RegionModel> MapAsync(Region region, CancellationToken cancellationToken)
  {
    return (await MapAsync([region], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<RegionModel>> MapAsync(IEnumerable<Region> regions, CancellationToken cancellationToken)
  {
    IEnumerable<Guid> userIds = regions.SelectMany(region => region.GetUserIds());
    IReadOnlyDictionary<Guid, Actor> actors = await _actorService.FindAsync(userIds, cancellationToken);
    Mapper mapper = new(actors);

    return regions.Select(mapper.ToRegion).ToList().AsReadOnly();
  }
}
