using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class WorldQuerier : IWorldQuerier
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;
  private readonly DbSet<WorldEntity> _worlds;

  public WorldQuerier(IActorService actorService, IContext context, PokemonContext pokemon, ISqlHelper sqlHelper)
  {
    _actorService = actorService;
    _context = context;
    _worlds = pokemon.Worlds;
    _sqlHelper = sqlHelper;
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken)
  {
    return await _worlds.CountAsync(x => x.OwnerId == _context.UserId.Value, cancellationToken);
  }

  public async Task<WorldModel> FindAsync(World world, CancellationToken cancellationToken)
  {
    return await FindAsync(world.Id, cancellationToken);
  }
  public async Task<WorldModel> FindAsync(WorldId id, CancellationToken cancellationToken)
  {
    WorldEntity world = await _worlds.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The world entity 'StreamId={id}' was not found.");
    return await MapAsync(world, cancellationToken);
  }

  public async Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.EntityId == id && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }

  public async Task<SearchResults<WorldModel>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Worlds.Table).SelectAll(Db.Worlds.Table)
        .Where(Db.Worlds.OwnerId, Operators.IsEqualTo(_context.UserId.Value))
        .ApplyIdFilter(Db.Worlds.EntityId, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Worlds.Key, Db.Worlds.Name);

    IQueryable<WorldEntity> query = _worlds.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<WorldEntity>? ordered = null;
    foreach (WorldSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case WorldSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case WorldSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case WorldSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case WorldSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    WorldEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<WorldModel> worlds = await MapAsync(entities, cancellationToken);

    return new SearchResults<WorldModel>(worlds, total);
  }

  public async Task<WorldId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _worlds.Where(x => x.Key == key.Value).Select(x => x.StreamId).SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new WorldId(streamId);
  }

  private async Task<WorldModel> MapAsync(WorldEntity world, CancellationToken cancellationToken)
  {
    return (await MapAsync([world], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<WorldModel>> MapAsync(IEnumerable<WorldEntity> worlds, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = worlds.SelectMany(world => world.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actorService.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return worlds.Select(mapper.ToWorld).ToList().AsReadOnly();
  }
}
