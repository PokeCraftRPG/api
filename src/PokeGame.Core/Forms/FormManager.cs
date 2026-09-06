using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms.Events;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public interface IFormManager
{
  Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken = default);
  Task<FormAbilities> ResolveAbilitiesAsync(FormAbilitiesPayload payload, string propertyName, CancellationToken cancellationToken = default);
  Task<FormSprites> ResolveSpritesAsync(FormSpritesPayload payload, string propertyName, CancellationToken cancellationToken = default);
}

internal class FormManager : IFormManager
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAssetRepository _assetRepository;
  private readonly IContext _context;
  private readonly IFormQuerier _formQuerier;

  public FormManager(IAbilityRepository abilityRepository, IAssetRepository assetRepository, IContext context, IFormQuerier formQuerier)
  {
    _abilityRepository = abilityRepository;
    _assetRepository = assetRepository;
    _context = context;
    _formQuerier = formQuerier;
  }

  public async Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in form.Changes)
    {
      if (change is FormCreated created)
      {
        key = created.Key;
      }
      else if (change is FormKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      FormId? formId = await _formQuerier.GetIdAsync(key, cancellationToken);
      if (formId.HasValue && !formId.Value.Equals(form.Id))
      {
        throw new KeyAlreadyUsedException(form, formId.Value.EntityId, form.Key, nameof(form.Key));
      }
    }
  }

  public async Task<FormAbilities> ResolveAbilitiesAsync(FormAbilitiesPayload payload, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    HashSet<AbilityId> abilityIds = new(capacity: 3);
    abilityIds.Add(new AbilityId(worldId, payload.PrimaryId));
    if (payload.SecondaryId.HasValue)
    {
      abilityIds.Add(new AbilityId(worldId, payload.SecondaryId.Value));
    }
    if (payload.HiddenId.HasValue)
    {
      abilityIds.Add(new AbilityId(worldId, payload.HiddenId.Value));
    }
    Dictionary<AbilityId, Ability> abilities = (await _abilityRepository.LoadAsync(abilityIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    AbilityId abilityId = new(worldId, payload.PrimaryId);
    Ability primary = abilities.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.PrimaryId)}");
    Ability? secondary = null;
    Ability? hidden = null;
    if (payload.SecondaryId.HasValue)
    {
      abilityId = new AbilityId(worldId, payload.SecondaryId.Value);
      secondary = abilities.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.SecondaryId)}");
    }
    if (payload.HiddenId.HasValue)
    {
      abilityId = new AbilityId(worldId, payload.HiddenId.Value);
      hidden = abilities.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.HiddenId)}");
    }
    return FormAbilities.From(primary, secondary, hidden);
  }

  public async Task<FormSprites> ResolveSpritesAsync(FormSpritesPayload payload, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    HashSet<AssetId> assetIds = new(capacity: 4);
    assetIds.Add(new AssetId(worldId, payload.DefaultId));
    if (payload.ShinyId.HasValue)
    {
      assetIds.Add(new AssetId(worldId, payload.ShinyId.Value));
    }
    if (payload.FemaleId.HasValue)
    {
      assetIds.Add(new AssetId(worldId, payload.FemaleId.Value));
    }
    if (payload.FemaleShinyId.HasValue)
    {
      assetIds.Add(new AssetId(worldId, payload.FemaleShinyId.Value));
    }
    Dictionary<AssetId, Asset> assets = (await _assetRepository.LoadAsync(assetIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    AssetId assetId = new(worldId, payload.DefaultId);
    Asset @default = assets.GetValueOrDefault(assetId) ?? throw new EntityNotFoundException(assetId, $"{propertyName}.{nameof(payload.DefaultId)}");
    Asset? shiny = null;
    Asset? female = null;
    Asset? femaleShiny = null;
    if (payload.ShinyId.HasValue)
    {
      assetId = new AssetId(worldId, payload.ShinyId.Value);
      shiny = assets.GetValueOrDefault(assetId) ?? throw new EntityNotFoundException(assetId, $"{propertyName}.{nameof(payload.ShinyId)}");
    }
    if (payload.FemaleId.HasValue)
    {
      assetId = new AssetId(worldId, payload.FemaleId.Value);
      female = assets.GetValueOrDefault(assetId) ?? throw new EntityNotFoundException(assetId, $"{propertyName}.{nameof(payload.FemaleId)}");
    }
    if (payload.FemaleShinyId.HasValue)
    {
      assetId = new AssetId(worldId, payload.FemaleShinyId.Value);
      femaleShiny = assets.GetValueOrDefault(assetId) ?? throw new EntityNotFoundException(assetId, $"{propertyName}.{nameof(payload.FemaleShinyId)}");
    }
    return FormSprites.From(@default, shiny, female, femaleShiny);
  }
}
