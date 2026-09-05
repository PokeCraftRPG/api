using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Moves.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class MoveEvents :
  IEventHandler<MoveCreated>,
  IEventHandler<MoveDeleted>,
  IEventHandler<MoveDetailsChanged>,
  IEventHandler<MoveKeyChanged>,
  IEventHandler<MoveMechanicsChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MoveCreated>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDeleted>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDetailsChanged>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveKeyChanged>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveMechanicsChanged>, MoveEvents>();
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
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);

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

  public async Task HandleAsync(MoveDetailsChanged @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is not null && move.Version == (@event.Version - 1))
    {
      move.SetDetails(@event);

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

  public async Task HandleAsync(MoveMechanicsChanged @event, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (move is not null && move.Version == (@event.Version - 1))
    {
      move.SetMechanics(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
