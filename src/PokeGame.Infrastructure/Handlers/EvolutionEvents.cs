using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
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
      int? itemId = @event.ItemId.HasValue ? await FindItemIdAsync(@event.ItemId.Value, cancellationToken) : null;
      int? moveId = @event.MoveId.HasValue ? await FindMoveIdAsync(@event.MoveId.Value, cancellationToken) : null;

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
      int sourceId = await FindFormIdAsync(@event.SourceId, cancellationToken);
      int targetId = await FindFormIdAsync(@event.TargetId, cancellationToken);
      int? itemId = @event.ItemId.HasValue ? await FindItemIdAsync(@event.ItemId.Value, cancellationToken) : null;

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

  private async Task<int> FindFormIdAsync(FormId formId, CancellationToken cancellationToken)
  {
    return await _pokemon.Forms
      .Where(x => x.StreamId == formId.Value)
      .Select(x => (int?)x.FormId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The form entity 'StreamId={formId}' was not found.");
  }

  private async Task<int> FindItemIdAsync(ItemId itemId, CancellationToken cancellationToken)
  {
    return await _pokemon.Items
      .Where(x => x.StreamId == itemId.Value)
      .Select(x => (int?)x.ItemId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The item entity 'StreamId={itemId}' was not found.");
  }

  private async Task<int> FindMoveIdAsync(MoveId moveId, CancellationToken cancellationToken)
  {
    return await _pokemon.Moves
      .Where(x => x.StreamId == moveId.Value)
      .Select(x => (int?)x.MoveId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The move entity 'StreamId={moveId}' was not found.");
  }
}
