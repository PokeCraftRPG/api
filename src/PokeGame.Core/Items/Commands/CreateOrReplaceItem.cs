using Logitar.CQRS;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Moves;
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
  private readonly IMoveManager _moveManager;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public CreateOrReplaceItemCommandHandler(
    IContext context,
    IItemQuerier itemQuerier,
    IItemRepository itemRepository,
    IMoveManager moveManager,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _context = context;
    _itemQuerier = itemQuerier;
    _itemRepository = itemRepository;
    _moveManager = moveManager;
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
    ItemProperties properties = await GetPropertiesAsync(payload, cancellationToken);

    bool created = false;
    if (item is null)
    {
      await _permissionService.CheckAsync(Actions.CreateItem, cancellationToken);

      item = new(key, properties, userId, itemId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, item, cancellationToken);

      if (properties.Category != item.Category)
      {
        throw new ImmutablePropertyException<ItemCategory>(item, item.Category, properties.Category, properties.Category.ToString());
      }

      item.SetKey(key, userId);
      item.SetProperties(properties, userId);
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

  private async Task<ItemProperties> GetPropertiesAsync(CreateOrReplaceItemPayload payload, CancellationToken cancellationToken)
  {
    List<ItemProperties> properties = new(capacity: 9);

    if (payload.BattleItem is not null)
    {
      properties.Add(new BattleItemProperties(payload.BattleItem));
    }
    if (payload.Berry is not null)
    {
      properties.Add(new BerryProperties(payload.Berry));
    }
    if (payload.KeyItem is not null)
    {
      properties.Add(new KeyItemProperties(payload.KeyItem));
    }
    if (payload.Material is not null)
    {
      properties.Add(new MaterialProperties(payload.Material));
    }
    if (payload.Medicine is not null)
    {
      properties.Add(new MedicineProperties(payload.Medicine));
    }
    if (payload.OtherItem is not null)
    {
      properties.Add(new OtherItemProperties(payload.OtherItem));
    }
    if (payload.PokeBall is not null)
    {
      properties.Add(new PokeBallProperties(payload.PokeBall));
    }
    if (payload.TechnicalMachine is not null)
    {
      string propertyName = string.Join('.', nameof(payload.TechnicalMachine), nameof(payload.TechnicalMachine.Move));
      Move move = await _moveManager.FindAsync(payload.TechnicalMachine.Move, propertyName, cancellationToken);
      properties.Add(new TechnicalMachineProperties(move));
    }
    if (payload.Treasure is not null)
    {
      properties.Add(new TreasureProperties(payload.Treasure));
    }

    if (properties.Count > 1)
    {
      throw new ArgumentException("Many properties were provided, exactly one is expected.", nameof(payload));
    }
    return properties.SingleOrDefault() ?? throw new ArgumentException("No property was provided, exactly one is expected.", nameof(payload));
  }
}
