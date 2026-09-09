using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Items.Commands;

internal record UpdateItemCommand(Guid Id, UpdateItemPayload Payload) : ICommand<ItemDto?>;

internal class UpdateItemCommandHandler : ICommandHandler<UpdateItemCommand, ItemDto?>
{
  private readonly IContext _context;
  private readonly IItemManager _itemManager;
  private readonly IItemQuerier _itemQuerier;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;

  public UpdateItemCommandHandler(
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

  public async Task<ItemDto?> HandleAsync(UpdateItemCommand command, CancellationToken cancellationToken)
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

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      item.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Name is not null || payload.Summary is not null || payload.Content is not null)
    {
      item.SetDetails(
        payload.Name is null ? item.Name : Name.TryCreate(payload.Name.Value),
        payload.Summary is null ? item.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? item.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.Price is not null || payload.Weight is not null)
    {
      item.SetCharacteristics(
        payload.Price is null ? item.Price : Price.TryCreate(payload.Price.Value),
        payload.Weight is null ? item.Weight : Weight.TryCreate(payload.Weight.Value),
        actorId);
    }

    if (payload.SpriteId is not null)
    {
      await _itemManager.SetSpriteAsync(item, payload.SpriteId.Value, nameof(payload.SpriteId), cancellationToken);
    }

    await _itemManager.EnsureUnicityAsync(item, cancellationToken);
    await _itemRepository.SaveAsync(item, cancellationToken);

    return await _itemQuerier.ReadAsync(item, cancellationToken);
  }
}
