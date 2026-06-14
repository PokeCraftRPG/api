using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;
using PokeGame.Infrastructure.Outbox;

namespace PokeGame.Infrastructure.Handlers;

internal class AbilityEvents : IEventHandler<AbilityCreated>,
  IEventHandler<AbilityDeleted>,
  IEventHandler<AbilityDescribed>,
  IEventHandler<AbilityKeyChanged>,
  IEventHandler<AbilityRenamed>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<AbilityCreated>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityDeleted>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityDescribed>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityKeyChanged>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityRenamed>, AbilityEvents>();
  }

  private readonly IOutboxService _outbox;
  private readonly PokemonContext _pokemon;

  public AbilityEvents(IOutboxService outbox, PokemonContext pokemon)
  {
    _outbox = outbox;
    _pokemon = pokemon;
  }

  public async Task HandleAsync(AbilityCreated @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      AbilityEntity? ability = await _pokemon.Abilities.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (ability is null)
      {
        WorldId worldId = new AbilityId(@event.StreamId).WorldId;
        WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

        ability = new AbilityEntity(world, @event);

        _pokemon.Abilities.Add(ability);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(AbilityDeleted @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (ability is not null)
      {
        _pokemon.Abilities.Remove(ability);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(AbilityDescribed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, ability);

      ability.Describe(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(AbilityKeyChanged @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, ability);

      ability.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(AbilityRenamed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, ability);

      ability.Rename(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);
}
