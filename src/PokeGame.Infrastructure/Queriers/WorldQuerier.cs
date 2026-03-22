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
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<WorldEntity> _worlds;

  public WorldQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _worlds = pokemon.Worlds;
  }

  public async Task EnsureUnicityAsync(World world, CancellationToken cancellationToken)
  {
    string? streamId = await _worlds.Where(x => x.Key == world.Key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    if (streamId is not null && streamId != world.Id.Value)
    {
      throw new PropertyConflictException<string>(world, new WorldId(streamId).ToGuid(), world.Key.Value, nameof(World.Key));
    }
  }

  public async Task<WorldModel> ReadAsync(World world, CancellationToken cancellationToken)
  {
    return await ReadAsync(world.Id, cancellationToken) ?? throw new InvalidOperationException($"The world entity '{world}' was not found.");
  }
  public async Task<WorldModel?> ReadAsync(WorldId id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Id == id && x.OwnerId == _context.UserId.Value)
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
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return worlds.Select(mapper.ToWorld).ToList().AsReadOnly();
  }
}
