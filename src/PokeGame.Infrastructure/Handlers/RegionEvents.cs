using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class RegionEvents : IEventHandler<RegionCreated>, IEventHandler<RegionDeleted>, IEventHandler<RegionUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RegionCreated>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionDeleted>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionUpdated>, RegionEvents>();
  }

  private readonly PokemonContext _pokemon;

  public RegionEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(RegionCreated @event, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _pokemon.Regions.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (region is null)
    {
      WorldId worldId = new RegionId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      region = new(world, @event);

      _pokemon.Regions.Add(region);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RegionDeleted @event, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (region is not null)
    {
      _pokemon.Regions.Remove(region);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(RegionUpdated @event, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (region is not null && region.Version == (@event.Version - 1))
    {
      region.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
