using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class SpeciesEvents :
  IEventHandler<SpeciesCreated>,
  IEventHandler<SpeciesDeleted>,
  IEventHandler<SpeciesKeyChanged>,
  IEventHandler<SpeciesRegionalNumberChanged>,
  IEventHandler<SpeciesRegionalNumberRemoved>,
  IEventHandler<SpeciesUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<SpeciesCreated>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesDeleted>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesKeyChanged>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesRegionalNumberChanged>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesRegionalNumberRemoved>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesUpdated>, SpeciesEvents>();
  }

  private readonly PokemonContext _pokemon;

  public SpeciesEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(SpeciesCreated @event, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _pokemon.Species.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is null)
    {
      int worldId = await _pokemon.FindWorldIdAsync(new SpeciesId(@event.StreamId).WorldId, cancellationToken);

      species = new SpeciesEntity(worldId, @event);

      _pokemon.Species.Add(species);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(SpeciesDeleted @event, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _pokemon.Species.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is not null)
    {
      _pokemon.Species.Remove(species);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(SpeciesKeyChanged @event, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _pokemon.Species.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is not null && species.Version == (@event.Version - 1))
    {
      species.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(SpeciesRegionalNumberChanged @event, CancellationToken cancellationToken)
  {
    int regionId = await _pokemon.Regions.Where(x => x.StreamId == @event.RegionId.Value)
      .Select(x => (int?)x.RegionId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The region entity 'StreamId={@event.RegionId}' was not found.");

    SpeciesEntity? species = await _pokemon.Species
      .Include(x => x.RegionalNumbers.Where(y => y.RegionId == regionId))
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is not null && species.Version == (@event.Version - 1))
    {
      species.SetRegionalNumber(regionId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(SpeciesRegionalNumberRemoved @event, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _pokemon.Species
      .Include(x => x.RegionalNumbers.Where(y => y.Region!.StreamId == @event.RegionId.Value))
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is not null && species.Version == (@event.Version - 1))
    {
      species.Update(@event);

      _pokemon.RegionalNumbers.RemoveRange(species.RegionalNumbers);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(SpeciesUpdated @event, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _pokemon.Species.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (species is not null && species.Version == (@event.Version - 1))
    {
      species.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
