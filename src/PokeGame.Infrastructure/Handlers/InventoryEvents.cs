using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Events;
using PokeGame.Core.Trainers;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class InventoryEvents :
  IEventHandler<InventoryItemAdded>,
  IEventHandler<InventoryItemChanged>,
  IEventHandler<InventoryItemRemoved>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<InventoryItemAdded>, InventoryEvents>();
    services.AddTransient<IEventHandler<InventoryItemChanged>, InventoryEvents>();
    services.AddTransient<IEventHandler<InventoryItemRemoved>, InventoryEvents>();
  }

  private readonly PokemonContext _pokemon;

  public InventoryEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(InventoryItemAdded @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryItemEntity? inventory = await _pokemon.Inventory
      .SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value, cancellationToken);
    if (inventory is null)
    {
      int trainerKey = await _pokemon.FindTrainerIdAsync(trainerId, cancellationToken);
      int itemId = await _pokemon.FindItemIdAsync(@event.ItemId, cancellationToken);

      inventory = new InventoryItemEntity(trainerKey, itemId, @event.Quantity);

      _pokemon.Inventory.Add(inventory);
    }
    else
    {
      inventory.Quantity = @event.Quantity;
    }
    await _pokemon.SaveChangesAsync(cancellationToken);
  }

  public async Task HandleAsync(InventoryItemChanged @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryItemEntity? inventory = await _pokemon.Inventory
      .SingleOrDefaultAsync(x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value, cancellationToken);
    if (inventory is null)
    {
      int trainerKey = await _pokemon.FindTrainerIdAsync(trainerId, cancellationToken);
      int itemId = await _pokemon.FindItemIdAsync(@event.ItemId, cancellationToken);

      inventory = new InventoryItemEntity(trainerKey, itemId, @event.Quantity);

      _pokemon.Inventory.Add(inventory);
    }
    else
    {
      inventory.Quantity = @event.Quantity;
    }
    await _pokemon.SaveChangesAsync(cancellationToken);
  }

  public async Task HandleAsync(InventoryItemRemoved @event, CancellationToken cancellationToken)
  {
    TrainerId trainerId = new InventoryId(@event.StreamId).TrainerId;
    InventoryItemEntity? inventory = await _pokemon.Inventory.SingleOrDefaultAsync(
      x => x.Trainer!.StreamId == trainerId.Value && x.Item!.StreamId == @event.ItemId.Value,
      cancellationToken);
    if (inventory is not null)
    {
      _pokemon.Inventory.Remove(inventory);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
