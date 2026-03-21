using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class AbilityEvents : IEventHandler<AbilityCreated>, IEventHandler<AbilityDeleted>, IEventHandler<AbilityUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<AbilityCreated>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityDeleted>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityUpdated>, AbilityEvents>();
  }

  private readonly PokemonContext _pokemon;

  public AbilityEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(AbilityCreated @event, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _pokemon.Abilities.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (ability is null)
    {
      WorldId worldId = new AbilityId(@event.StreamId).WorldId;
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == worldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");

      ability = new(world, @event);

      _pokemon.Abilities.Add(ability);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(AbilityDeleted @event, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (ability is not null)
    {
      _pokemon.Abilities.Remove(ability);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(AbilityUpdated @event, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (ability is not null && ability.Version == (@event.Version - 1))
    {
      ability.Update(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
