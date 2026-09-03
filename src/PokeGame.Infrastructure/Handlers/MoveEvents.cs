using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class MoveEvents :
  IEventHandler<MoveCreated>,
  IEventHandler<MoveDeleted>,
  IEventHandler<MoveKeyChanged>,
  IEventHandler<MoveUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MoveCreated>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDeleted>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveKeyChanged>, MoveEvents>();
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
      int worldId = await _pokemon.FindWorldIdAsync(new MoveId(@event.StreamId).WorldId, cancellationToken);

      move = new MoveEntity(worldId, @event);

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

  public async Task HandleAsync(MoveKeyChanged @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is not null && move.Version == (@event.Version - 1))
    {
      move.SetKey(@event);

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
