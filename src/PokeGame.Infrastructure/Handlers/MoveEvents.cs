using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;
using PokeGame.Infrastructure.Outbox;

namespace PokeGame.Infrastructure.Handlers;

internal class MoveEvents : IEventHandler<MoveCreated>,
  IEventHandler<MoveDeleted>,
  IEventHandler<MoveDescribed>,
  IEventHandler<MoveGameDataChanged>,
  IEventHandler<MoveKeyChanged>,
  IEventHandler<MoveRenamed>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<MoveCreated>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDeleted>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveDescribed>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveGameDataChanged>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveKeyChanged>, MoveEvents>();
    services.AddTransient<IEventHandler<MoveRenamed>, MoveEvents>();
  }

  private readonly IOutboxService _outbox;
  private readonly PokemonContext _pokemon;

  public MoveEvents(IOutboxService outbox, PokemonContext pokemon)
  {
    _outbox = outbox;
    _pokemon = pokemon;
  }

  public async Task HandleAsync(MoveCreated @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (move is null)
      {
        WorldId worldId = new MoveId(@event.StreamId).WorldId;
        WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

        move = new MoveEntity(world, @event);

        _pokemon.Moves.Add(move);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(MoveDeleted @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (move is not null)
      {
        _pokemon.Moves.Remove(move);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(MoveDescribed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, move);

      move.Describe(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(MoveGameDataChanged @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, move);

      move.SetGameData(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(MoveKeyChanged @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, move);

      move.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(MoveRenamed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      MoveEntity? move = await _pokemon.Moves.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, move);

      move.Rename(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);
}
