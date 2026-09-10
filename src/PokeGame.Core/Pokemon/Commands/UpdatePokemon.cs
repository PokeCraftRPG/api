using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Assets;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon.Commands;

internal record UpdatePokemonCommand(Guid Id, UpdatePokemonPayload Payload) : ICommand<PokemonDto?>;

internal class UpdatePokemonCommandHandler : ICommandHandler<UpdatePokemonCommand, PokemonDto?>
{
  private readonly IAssetRepository _assetRepository;
  private readonly IContext _context;
  private readonly IItemRepository _itemRepository;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonManager _pokemonManager;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;

  public UpdatePokemonCommandHandler(
    IAssetRepository assetRepository,
    IContext context,
    IItemRepository itemRepository,
    IPermissionService permissionService,
    IPokemonManager pokemonManager,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository)
  {
    _assetRepository = assetRepository;
    _context = context;
    _itemRepository = itemRepository;
    _permissionService = permissionService;
    _pokemonManager = pokemonManager;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
  }

  public async Task<PokemonDto?> HandleAsync(UpdatePokemonCommand command, CancellationToken cancellationToken)
  {
    UpdatePokemonPayload payload = command.Payload;
    payload.Validate();

    PokemonId pokemonId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, specimen, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      specimen.SetKey(new Key(payload.Key), actorId);
    }

    if (payload.Nickname is not null)
    {
      specimen.SetNickname(Name.TryCreate(payload.Nickname.Value), actorId);
    }

    if (payload.Summary is not null || payload.Content is not null)
    {
      specimen.SetDetails(
        payload.Summary is null ? specimen.Summary : Summary.TryCreate(payload.Summary.Value),
        payload.Content is null ? specimen.Content : Content.TryCreate(payload.Content.Value),
        actorId);
    }

    if (payload.Vitality is not null || payload.Stamina is not null || payload.Condition is not null || payload.Friendship is not null)
    {
      specimen.SetStatus(
        payload.Vitality ?? specimen.Vitality,
        payload.Stamina ?? specimen.Stamina,
        payload.Condition is null ? specimen.Condition : payload.Condition.Value,
        payload.Friendship.HasValue ? new Friendship(payload.Friendship.Value) : specimen.Friendship,
        actorId);
    }

    if (payload.HeldItemId is not null)
    {
      Item? heldItem = null;
      if (payload.HeldItemId.Value.HasValue)
      {
        ItemId itemId = new(specimen.WorldId, payload.HeldItemId.Value.Value);
        heldItem = await _itemRepository.LoadAsync(itemId, cancellationToken) ?? throw new EntityNotFoundException(itemId, nameof(payload.HeldItemId));
      }
      specimen.SetHeldItem(heldItem, actorId);
    }

    if (payload.SpriteId is not null)
    {
      Asset? sprite = null;
      if (payload.SpriteId.Value.HasValue)
      {
        AssetId assetId = new(specimen.WorldId, payload.SpriteId.Value.Value);
        sprite = await _assetRepository.LoadAsync(assetId, cancellationToken) ?? throw new EntityNotFoundException(assetId, nameof(payload.SpriteId));
      }
      specimen.SetSprite(sprite, actorId);
    }

    await _pokemonManager.EnsureUnicityAsync(specimen, cancellationToken);
    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
