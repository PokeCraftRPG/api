using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;
using PokeGame.Infrastructure.Outbox;

namespace PokeGame.Infrastructure.Handlers;

internal class RegionEvents : IEventHandler<RegionCreated>,
  IEventHandler<RegionDeleted>,
  IEventHandler<RegionDescribed>,
  IEventHandler<RegionKeyChanged>,
  IEventHandler<RegionRenamed>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RegionCreated>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionDeleted>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionDescribed>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionKeyChanged>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionRenamed>, RegionEvents>();
  }

  private readonly IOutboxService _outbox;
  private readonly PokemonContext _pokemon;

  public RegionEvents(IOutboxService outbox, PokemonContext pokemon)
  {
    _outbox = outbox;
    _pokemon = pokemon;
  }

  public async Task HandleAsync(RegionCreated @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      RegionEntity? region = await _pokemon.Regions.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (region is null)
      {
        WorldId worldId = new RegionId(@event.StreamId).WorldId;
        WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
          ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

        region = new RegionEntity(world, @event);

        _pokemon.Regions.Add(region);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(RegionDeleted @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      if (region is not null)
      {
        _pokemon.Regions.Remove(region);

        await _pokemon.SaveChangesAsync(cancellationToken);
      }
    },
    cancellationToken);

  public async Task HandleAsync(RegionDescribed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, region);

      region.Describe(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(RegionKeyChanged @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, region);

      region.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);

  public async Task HandleAsync(RegionRenamed @event, CancellationToken cancellationToken) => await _outbox.HandleAsync(
    @event,
    async (@event, cancellationToken) =>
    {
      RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
      UnexpectedVersionException.ThrowIfUnexpected(@event, region);

      region.Rename(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    },
    cancellationToken);
}
