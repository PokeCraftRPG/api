using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Items.Commands;

internal record CreateOrReplaceItemCommand(CreateOrReplaceItemPayload Payload, Guid? Id) : ICommand<CreateOrReplaceItemResult>;

internal class CreateOrReplaceItemCommandHandler : ICommandHandler<CreateOrReplaceItemCommand, CreateOrReplaceItemResult>
{
  private readonly IContext _context;
  private readonly IItemManager _itemManager;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;

  public CreateOrReplaceItemCommandHandler(
    IContext context,
    IItemManager itemManager,
    IItemQuerier itemQuerier,
    IItemRepository itemRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _itemManager = itemManager;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
  }

  public async Task<CreateOrReplaceItemResult> HandleAsync(CreateOrReplaceItemCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemPayload payload = command.Payload;
    payload.Validate();

    ItemId itemId = ItemId.NewId(_context.WorldId);
    Item? item = null;
    if (command.Id.HasValue)
    {
      itemId = new ItemId(itemId.WorldId, command.Id.Value);
      item = await _itemRepository.LoadAsync(itemId, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

    bool created = false;
    if (item is null)
    {
      await _permissionService.CheckAsync(Actions.CreateItem, cancellationToken);

      item = new Item(itemId, payload.Category, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, item, cancellationToken);

      if (payload.Category != item.Category)
      {
        throw new ImmutablePropertyException<ItemCategory>(item, item.Category, payload.Category, nameof(payload.Category));
      }

      item.SetKey(key, actorId);
    }

    item.SetDetails(Name.TryCreate(payload.Name), Summary.TryCreate(payload.Summary), Content.TryCreate(payload.Content), actorId);
    item.SetCharacteristics(Price.TryCreate(payload.Price), Weight.TryCreate(payload.Weight), actorId);
    await _itemManager.SetSpriteAsync(item, payload.SpriteId, nameof(payload.SpriteId), cancellationToken);

    await _itemManager.EnsureUnicityAsync(item, cancellationToken);
    await _itemRepository.SaveAsync(item, cancellationToken);

    ItemDto dto = await _itemQuerier.ReadAsync(item, cancellationToken);
    return new CreateOrReplaceItemResult(dto, created);
  }
}
