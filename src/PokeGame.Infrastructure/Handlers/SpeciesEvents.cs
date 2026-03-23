using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class SpeciesEvents : IEventHandler<SpeciesCreated>, IEventHandler<SpeciesDeleted>, IEventHandler<SpeciesKeyChanged>, IEventHandler<SpeciesUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<SpeciesCreated>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesDeleted>, SpeciesEvents>();
    services.AddTransient<IEventHandler<SpeciesKeyChanged>, SpeciesEvents>();
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
      WorldId worldId = new SpeciesId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      species = new(world, @event);

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
