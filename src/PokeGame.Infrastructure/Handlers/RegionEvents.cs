using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Regions.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class RegionEvents :
  IEventHandler<RegionCreated>,
  IEventHandler<RegionDeleted>,
  IEventHandler<RegionKeyChanged>,
  IEventHandler<RegionUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<RegionCreated>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionDeleted>, RegionEvents>();
    services.AddTransient<IEventHandler<RegionKeyChanged>, RegionEvents>();
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
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);

      region = new RegionEntity(worldId, @event);

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

  public async Task HandleAsync(RegionKeyChanged @event, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _pokemon.Regions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (region is not null && region.Version == (@event.Version - 1))
    {
      region.SetKey(@event);

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
