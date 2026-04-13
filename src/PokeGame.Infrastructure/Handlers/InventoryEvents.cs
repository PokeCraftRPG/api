using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Events;
using PokeGame.Core.Trainers;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class InventoryEvents : IEventHandler<InventoryItemAdded>, IEventHandler<InventoryItemRemoved>, IEventHandler<InventoryItemUpdated>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<InventoryItemAdded>, InventoryEvents>();
    services.AddTransient<IEventHandler<InventoryItemRemoved>, InventoryEvents>();
    services.AddTransient<IEventHandler<InventoryItemUpdated>, InventoryEvents>();
  }

  private readonly PokemonContext _pokemon;

  public InventoryEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(InventoryItemAdded @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryEntity? inventory = await _pokemon.Inventory.SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value, cancellationToken);

    if (inventory is null)
    {
      TrainerEntity trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == trainerId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The trainer entity 'StreamId={trainerId}' was not found.");

      ItemEntity item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.ItemId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The item entity 'StreamId={@event.ItemId}' was not found.");

      inventory = new InventoryEntity(trainer, item, @event);

      _pokemon.Inventory.Add(inventory);
    }
    else
    {
      inventory.Add(@event);
    }

    await _pokemon.SaveChangesAsync(cancellationToken);
  }

  public async Task HandleAsync(InventoryItemRemoved @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryEntity? inventory = await _pokemon.Inventory.SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value, cancellationToken);

    if (inventory is not null)
    {
      inventory.Remove(@event);

      if (inventory.Quantity <= 0)
      {
        _pokemon.Inventory.Remove(inventory);
      }

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(InventoryItemUpdated @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryEntity? inventory = await _pokemon.Inventory.SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value, cancellationToken);

    if (inventory is null)
    {
      TrainerEntity trainer = await _pokemon.Trainers.SingleOrDefaultAsync(x => x.StreamId == trainerId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The trainer entity 'StreamId={trainerId}' was not found.");

      ItemEntity item = await _pokemon.Items.SingleOrDefaultAsync(x => x.StreamId == @event.ItemId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The item entity 'StreamId={@event.ItemId}' was not found.");

      inventory = new InventoryEntity(trainer, item, @event);

      _pokemon.Inventory.Add(inventory);
    }
    else
    {
      inventory.Update(@event);
    }

    await _pokemon.SaveChangesAsync(cancellationToken);
  }
}
