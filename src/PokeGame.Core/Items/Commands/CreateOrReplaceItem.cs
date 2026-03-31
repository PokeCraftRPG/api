using Logitar.CQRS;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items.Commands;

internal record CreateOrReplaceItemCommand(CreateOrReplaceItemPayload Payload, Guid? Id) : ICommand<CreateOrReplaceItemResult>;

internal class CreateOrReplaceItemCommandHandler : ICommandHandler<CreateOrReplaceItemCommand, CreateOrReplaceItemResult>
{
  private readonly IContext _context;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public CreateOrReplaceItemCommandHandler(
    IContext context,
    IItemQuerier itemQuerier,
    IItemRepository itemRepository,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
    _storageService = storageService;
  }

  public async Task<CreateOrReplaceItemResult> HandleAsync(CreateOrReplaceItemCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    ItemId itemId = ItemId.NewId(worldId);
    Item? item = null;
    if (command.Id.HasValue)
    {
      itemId = new(worldId, command.Id.Value);
      item = await _itemRepository.LoadAsync(itemId, cancellationToken);
    }

    Slug key = new(payload.Key);

    bool created = false;
    if (item is null)
    {
      await _permissionService.CheckAsync(Actions.CreateItem, cancellationToken);

      item = new(key, userId, itemId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, item, cancellationToken);

      item.SetKey(key, userId);
    }

    item.Name = Name.TryCreate(payload.Name);
    item.Description = Description.TryCreate(payload.Description);

    item.Price = Price.TryCreate(payload.Price);

    item.Sprite = Url.TryCreate(payload.Sprite);
    item.Url = Url.TryCreate(payload.Url);
    item.Notes = Notes.TryCreate(payload.Notes);

    item.Update(userId);

    await _itemQuerier.EnsureUnicityAsync(item, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      item,
      async () => await _itemRepository.SaveAsync(item, cancellationToken),
      cancellationToken);

    ItemModel model = await _itemQuerier.ReadAsync(item, cancellationToken);
    return new CreateOrReplaceItemResult(model, created);
  }
}
