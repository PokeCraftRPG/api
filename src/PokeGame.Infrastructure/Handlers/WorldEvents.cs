using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds.Events;
using PokeGame.Infrastructure.Entities;
using PokeGame.Infrastructure.Outbox;

namespace PokeGame.Infrastructure.Handlers;

internal class WorldEvents : IEventHandler<WorldCreated>,
  IEventHandler<WorldDeleted>,
  IEventHandler<WorldDescribed>,
  IEventHandler<WorldKeyChanged>,
  IEventHandler<WorldRenamed>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<WorldCreated>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldDeleted>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldDescribed>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldKeyChanged>, WorldEvents>();
    services.AddTransient<IEventHandler<WorldRenamed>, WorldEvents>();
  }

  private readonly IOutboxService _outbox;
  private readonly PokemonContext _pokemon;

  public WorldEvents(IOutboxService outbox, PokemonContext pokemon)
  {
    _outbox = outbox;
    _pokemon = pokemon;
  }

  public async Task HandleAsync(WorldCreated @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      WorldEntity? world = await _pokemon.Worlds.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (world is null)
      {
        world = new WorldEntity(@event);

        _pokemon.Worlds.Add(world);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(WorldDeleted @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (world is not null)
      {
        _pokemon.Worlds.Remove(world);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(WorldDescribed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, world);

      world.Describe(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(WorldKeyChanged @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, world);

      world.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(WorldRenamed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      WorldEntity? world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, world);

      world.Rename(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);
}
