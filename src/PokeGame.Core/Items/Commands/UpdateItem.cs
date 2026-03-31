using Logitar.CQRS;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Items.Commands;

internal record UpdateItemCommand(Guid Id, UpdateItemPayload Payload) : ICommand<ItemModel?>;

internal class UpdateItemCommandHandler : ICommandHandler<UpdateItemCommand, ItemModel?>
{
  private readonly IContext _context;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateItemCommandHandler(
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

  public async Task<ItemModel?> HandleAsync(UpdateItemCommand command, CancellationToken cancellationToken)
  {
    UpdateItemPayload payload = command.Payload;
    payload.Validate();

    ItemId itemId = new(_context.WorldId, command.Id);
    Item? item = await _itemRepository.LoadAsync(itemId, cancellationToken);
    if (item is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, item, cancellationToken);

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      item.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      item.Name = Name.TryCreate(payload.Name.Value);
    }
    if (payload.Description is not null)
    {
      item.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.Price is not null)
    {
      item.Price = Price.TryCreate(payload.Price.Value);
    }

    if (payload.Sprite is not null)
    {
      item.Sprite = Url.TryCreate(payload.Sprite.Value);
    }
    if (payload.Url is not null)
    {
      item.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      item.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    item.Update(userId);

    await _itemQuerier.EnsureUnicityAsync(item, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      item,
      async () => await _itemRepository.SaveAsync(item, cancellationToken),
      cancellationToken);

    return await _itemQuerier.ReadAsync(item, cancellationToken);
  }
}
