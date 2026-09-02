using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class AbilityEvents : IEventHandler<AbilityCreated>,
  IEventHandler<AbilityDeleted>,
  IEventHandler<AbilityKeyChanged>,
  IEventHandler<AbilityUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<AbilityCreated>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityDeleted>, AbilityEvents>();
    services.AddTransient<IEventHandler<AbilityKeyChanged>, AbilityEvents>();
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
      int worldId = await _pokemon.FindWorldIdAsync(new AbilityId(@event.StreamId).WorldId, cancellationToken);

      ability = new AbilityEntity(worldId, @event);

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

  public async Task HandleAsync(AbilityKeyChanged @event, CancellationToken cancellationToken)
  {
    AbilityEntity? ability = await _pokemon.Abilities.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (ability is not null && ability.Version == (@event.Version - 1))
    {
      ability.SetKey(@event);

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
