using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory.Commands;

internal record SetInventoryItemCommand(Guid TrainerId, Guid ItemId, SetInventoryItemPayload Payload) : ICommand<InventoryItemDto>;

internal class SetInventoryItemCommandHandler : ICommandHandler<SetInventoryItemCommand, InventoryItemDto>
{
  private readonly IContext _context;
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;

  public SetInventoryItemCommandHandler(
    IContext context,
    IInventoryRepository inventoryRepository,
    IItemQuerier itemQuerier,
    IItemRepository itemRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _inventoryRepository = inventoryRepository;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
  }

  public async Task<InventoryItemDto> HandleAsync(SetInventoryItemCommand command, CancellationToken cancellationToken)
  {
    SetInventoryItemPayload payload = command.Payload;
    payload.Validate();

    TrainerId trainerId = new(_context.WorldId, command.TrainerId);
    InventoryId inventoryId = new(trainerId);
    TrainerInventory inventory = await _inventoryRepository.LoadAsync(inventoryId, cancellationToken)
      ?? throw new NotImplementedException(); // TODO(fpion): implement
    await _permissionService.CheckAsync(Actions.Update, inventory, cancellationToken);

    ItemId itemId = new(trainerId.WorldId, command.ItemId);
    Item item = await _itemRepository.LoadAsync(itemId, cancellationToken) ?? throw new EntityNotFoundException(itemId, nameof(command.ItemId));

    inventory.SetQuantity(item, payload.Quantity, _context.ActorId);

    await _inventoryRepository.SaveAsync(inventory, cancellationToken);

    return new InventoryItemDto
    {
      Item = await _itemQuerier.ReadAsync(item, cancellationToken),
      Quantity = inventory.Quantities.GetValueOrDefault(itemId)
    };
  }
}
