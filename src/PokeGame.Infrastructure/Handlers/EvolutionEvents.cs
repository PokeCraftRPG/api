using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class EvolutionEvents :
  IEventHandler<EvolutionConditionsChanged>,
  IEventHandler<EvolutionCreated>,
  IEventHandler<EvolutionDeleted>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<EvolutionConditionsChanged>, EvolutionEvents>();
    services.AddTransient<IEventHandler<EvolutionCreated>, EvolutionEvents>();
    services.AddTransient<IEventHandler<EvolutionDeleted>, EvolutionEvents>();
  }

  private readonly PokemonContext _pokemon;

  public EvolutionEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(EvolutionConditionsChanged @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is not null && evolution.Version == (@event.Version - 1))
    {
      int? itemId = @event.ItemId.HasValue ? await _pokemon.FindItemIdAsync(@event.ItemId.Value, cancellationToken) : null;
      int? moveId = @event.MoveId.HasValue ? await _pokemon.FindMoveIdAsync(@event.MoveId.Value, cancellationToken) : null;

      evolution.SetConditions(itemId, moveId, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(EvolutionCreated @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is null)
    {
      int worldId = await _pokemon.FindWorldIdAsync(@event.StreamId, cancellationToken);
      int sourceId = await _pokemon.FindFormIdAsync(@event.SourceId, cancellationToken);
      int targetId = await _pokemon.FindFormIdAsync(@event.TargetId, cancellationToken);
      int? itemId = @event.ItemId.HasValue ? await _pokemon.FindItemIdAsync(@event.ItemId.Value, cancellationToken) : null;

      evolution = new EvolutionEntity(worldId, sourceId, targetId, itemId, @event);

      _pokemon.Evolutions.Add(evolution);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(EvolutionDeleted @event, CancellationToken cancellationToken)
  {
    EvolutionEntity? evolution = await _pokemon.Evolutions.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (evolution is not null)
    {
      _pokemon.Evolutions.Remove(evolution);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
