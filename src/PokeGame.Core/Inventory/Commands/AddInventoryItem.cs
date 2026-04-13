using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Inventory.Commands;

internal record AddInventoryItemCommand(Guid TrainerId, Guid ItemId, InventoryQuantityPayload Payload) : ICommand<InventoryItemModel>;

internal class AddInventoryItemCommandHandler : ICommandHandler<AddInventoryItemCommand, InventoryItemModel>
{
  private readonly IContext _context;
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly ITrainerRepository _trainerRepository;

  public AddInventoryItemCommandHandler(
    IContext context,
    IInventoryRepository inventoryRepository,
    IItemQuerier itemQuerier,
    IItemRepository itemRepository,
    ITrainerRepository trainerRepository)
  {
    _context = context;
    _inventoryRepository = inventoryRepository;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<InventoryItemModel> HandleAsync(AddInventoryItemCommand command, CancellationToken cancellationToken)
  {
    InventoryQuantityPayload payload = command.Payload;
    payload.Validate();

    WorldId worldId = _context.WorldId;

    TrainerId trainerId = new(worldId, command.TrainerId);
    Trainer trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken)
      ?? throw new TrainerNotFoundException(worldId, command.TrainerId.ToString(), nameof(command.TrainerId));

    ItemId itemId = new(worldId, command.ItemId);
    Item item = await _itemRepository.LoadAsync(itemId, cancellationToken)
      ?? throw new ItemNotFoundException(worldId, command.ItemId.ToString(), nameof(command.ItemId));

    InventoryId inventoryId = new(trainer.Id);
    InventoryAggregate inventory = await _inventoryRepository.LoadAsync(inventoryId, cancellationToken) ?? new(trainer);

    inventory.Add(item, payload.Quantity, _context.UserId);
    await _inventoryRepository.SaveAsync(inventory, cancellationToken);

    ItemModel model = await _itemQuerier.ReadAsync(item, cancellationToken);
    return new InventoryItemModel(model, inventory.Quantities[item.Id]);
  }
}
