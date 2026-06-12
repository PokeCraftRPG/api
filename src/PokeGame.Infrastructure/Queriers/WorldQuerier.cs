using Krakenar.Contracts.Actors;
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
  private readonly DbSet<WorldEntity> _worlds;

  public WorldQuerier(IActorService actorService, IContext context, PokemonContext pokemon)
  {
    _actorService = actorService;
    _context = context;
    _worlds = pokemon.Worlds;
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

  public async Task<WorldId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _worlds.Where(x => x.Key == key.Value).Select(x => x.StreamId).SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new WorldId(streamId);
  }
}
