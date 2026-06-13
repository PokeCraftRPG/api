using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class RegionQuerier : IRegionQuerier
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly DbSet<RegionEntity> _regions;
  private readonly ISqlHelper _sqlHelper;

  public RegionQuerier(IActorService actorService, IContext context, PokemonContext pokemon, ISqlHelper sqlHelper)
  {
    _actorService = actorService;
    _context = context;
    _regions = pokemon.Regions;
    _sqlHelper = sqlHelper;
  }

  public async Task<RegionModel> FindAsync(Region region, CancellationToken cancellationToken)
  {
    return await FindAsync(region.Id, cancellationToken);
  }
  public async Task<RegionModel> FindAsync(RegionId id, CancellationToken cancellationToken)
  {
    RegionEntity region = await _regions.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The region entity 'StreamId={id}' was not found.");
    return await MapAsync(region, cancellationToken);
  }

  public async Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _regions.AsNoTracking()
      .Where(x => x.EntityId == id && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return region is null ? null : await MapAsync(region, cancellationToken);
  }
  public async Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _regions.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return region is null ? null : await MapAsync(region, cancellationToken);
  }

  public async Task<SearchResults<RegionModel>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Regions.Table).SelectAll(Db.Regions.Table)
      .ApplyWorldFilter(Db.Regions.WorldId, _context.WorldId)
      .ApplyIdFilter(Db.Regions.EntityId, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Regions.Key, Db.Regions.Name);

    IQueryable<RegionEntity> query = _regions.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<RegionEntity>? ordered = null;
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

    RegionEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<RegionModel> regions = await MapAsync(entities, cancellationToken);

    return new SearchResults<RegionModel>(regions, total);
  }

  public async Task<RegionId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _regions
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new RegionId(streamId);
  }

  private async Task<RegionModel> MapAsync(RegionEntity region, CancellationToken cancellationToken)
  {
    return (await MapAsync([region], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<RegionModel>> MapAsync(IEnumerable<RegionEntity> regions, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = regions.SelectMany(region => region.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actorService.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return regions.Select(mapper.ToRegion).ToList().AsReadOnly();
  }
}
