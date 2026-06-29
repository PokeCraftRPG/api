using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Repositories;

internal class WorldRepository : Repository, IWorldRepository
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;

  public WorldRepository(IActorService actorService, IContext context, PokemonContext database, ISqlHelper sqlHelper) : base(database)
  {
    _actorService = actorService;
    _context = context;
    _sqlHelper = sqlHelper;
  }

  public void Add(World world)
  {
    Database.Worlds.Add(world);
    base.RecordChange(new WorldCreated(world));
  }
  public void Remove(World world)
  {
    Database.Worlds.Remove(world);
    base.RecordChange(new WorldDeleted(world, _context.UserId));
  }
  public void Update(World world, WorldUpdated record)
  {
    Database.Worlds.Update(world);
    base.RecordChange(record);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken)
  {
    return await Database.Worlds.CountAsync(x => x.OwnerId == _context.UserId, cancellationToken);
  }

  public async Task EnsureUnicityAsync(World world, CancellationToken cancellationToken)
  {
    Guid? worldId = await Database.Worlds.Where(x => x.Key == world.Key)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (worldId.HasValue && !worldId.Value.Equals(world.Id))
    {
      throw new KeyAlreadyUsedException(world, worldId.Value, world.Key, nameof(World.Key));
    }
  }

  public async Task<World?> LoadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await Database.Worlds.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<WorldModel> ReadAsync(World world, CancellationToken cancellationToken)
  {
    return await ReadAsync(world.Id, cancellationToken) ?? throw new InvalidOperationException($"The world 'Id={world.Id}' was not found.");
  }
  public async Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    World? world = await Database.Worlds.AsNoTracking()
      .Where(x => x.Id == id && x.OwnerId == _context.UserId)
      .SingleOrDefaultAsync(cancellationToken);

    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    World? world = await Database.Worlds.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.OwnerId == _context.UserId)
      .SingleOrDefaultAsync(cancellationToken);

    return world is null ? null : await MapAsync(world, cancellationToken);
  }

  public async Task<SearchResults<WorldModel>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Worlds.Table).SelectAll(Db.Worlds.Table)
      .ApplyIdFilter(Db.Worlds.Id, payload.Ids)
      .Where();
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Worlds.Name);

    IQueryable<World> query = Database.Worlds.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<World>? ordered = null;
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

    World[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<WorldModel> worlds = await MapAsync(entities, cancellationToken);

    return new SearchResults<WorldModel>(worlds, total);
  }

  private async Task<WorldModel> MapAsync(World world, CancellationToken cancellationToken)
  {
    return (await MapAsync([world], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<WorldModel>> MapAsync(IEnumerable<World> worlds, CancellationToken cancellationToken)
  {
    IEnumerable<Guid> userIds = worlds.SelectMany(world => world.GetUserIds());
    IReadOnlyDictionary<Guid, Actor> actors = await _actorService.FindAsync(userIds, cancellationToken);
    Mapper mapper = new(actors);

    return worlds.Select(mapper.ToWorld).ToList().AsReadOnly();
  }
}
