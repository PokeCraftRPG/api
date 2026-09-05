using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class WorldEvents :
  IEventHandler<WorldCreated>,
  IEventHandler<WorldDeleted>,
  IEventHandler<WorldDetailsChanged>,
  IEventHandler<WorldKeyChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<WorldCreated>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldDeleted>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldDetailsChanged>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldKeyChanged>, WorldEvents>();
  }

  private readonly PokemonContext _pokemon;

  public WorldEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(WorldCreated @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is null)
    {
      world = new WorldEntity(@event);

      _pokemon.Worlds.Add(world);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldDeleted @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null)
    {
      _pokemon.Worlds.Remove(world);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldDetailsChanged @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.SetDetails(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(WorldKeyChanged @event, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (world is not null && world.Version == (@event.Version - 1))
    {
      world.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
