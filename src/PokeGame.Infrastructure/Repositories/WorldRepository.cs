using Krakenar.Contracts.Actors;
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

  public WorldRepository(IActorService actorService, IContext context, PokemonContext database) : base(database)
  {
    _actorService = actorService;
    _context = context;
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
