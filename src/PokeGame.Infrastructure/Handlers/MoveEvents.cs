using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class MoveEvents : IEventHandler<MoveCreated>, IEventHandler<MoveDeleted>, IEventHandler<MoveUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MoveCreated>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDeleted>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveUpdated>, MoveEvents>();
  }

  private readonly PokemonContext _pokemon;

  public MoveEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(MoveCreated @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is null)
    {
      WorldId worldId = new MoveId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      move = new(world, @event);

      _pokemon.Moves.Add(move);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MoveDeleted @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is not null)
    {
      _pokemon.Moves.Remove(move);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(MoveUpdated @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is not null && move.Version == (@event.Version - 1))
    {
      move.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
