using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class FormEvents :
  IEventHandler<FormCharacteristicsChanged>,
  IEventHandler<FormCreated>,
  IEventHandler<FormDeleted>,
  IEventHandler<FormDetailsChanged>,
  IEventHandler<FormKeyChanged>,
  IEventHandler<FormSpritesChanged>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<FormCharacteristicsChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormCreated>, FormEvents>();
    services.AddTransient<IEventHandler<FormDeleted>, FormEvents>();
    services.AddTransient<IEventHandler<FormDetailsChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormKeyChanged>, FormEvents>();
    services.AddTransient<IEventHandler<FormSpritesChanged>, FormEvents>();
  }

  private readonly PokemonContext _pokemon;

  public FormEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(FormCharacteristicsChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      IReadOnlyDictionary<AbilitySlot, int> abilityIds = await FindAbilityIdsAsync(@event.Abilities, cancellationToken);

      form.SetCharacteristics(abilityIds, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormCreated @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is null)
    {
      var variety = await _pokemon.Varieties
        .Where(x => x.StreamId == @event.VarietyId.Value)
        .Select(x => new { x.VarietyId, x.WorldId })
        .SingleOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"The variety entity 'StreamId={@event.VarietyId}' was not found.");

      IReadOnlyDictionary<AbilitySlot, int> abilityIds = await FindAbilityIdsAsync(@event.Abilities, cancellationToken);

      form = new FormEntity(variety.WorldId, variety.VarietyId, abilityIds, @event);

      _pokemon.Forms.Add(form);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormDeleted @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null)
    {
      _pokemon.Forms.Remove(form);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormDetailsChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetDetails(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormKeyChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      form.SetKey(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(FormSpritesChanged @event, CancellationToken cancellationToken)
  {
    FormEntity? form = await _pokemon.Forms.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (form is not null && form.Version == (@event.Version - 1))
    {
      IReadOnlyDictionary<FormSpriteKind, int> assetIds = @event.Sprites is null
        ? new Dictionary<FormSpriteKind, int>()
        : await FindAssetIdsAsync(@event.Sprites, cancellationToken);

      form.SetSprites(assetIds, @event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  private async Task<IReadOnlyDictionary<AbilitySlot, int>> FindAbilityIdsAsync(FormAbilities abilities, CancellationToken cancellationToken)
  {
    HashSet<string> streamIds = new(capacity: 3);
    streamIds.Add(abilities.PrimaryId.Value);
    if (abilities.SecondaryId.HasValue)
    {
      streamIds.Add(abilities.SecondaryId.Value.Value);
    }
    if (abilities.HiddenId.HasValue)
    {
      streamIds.Add(abilities.HiddenId.Value.Value);
    }
    Dictionary<string, int> foundAbilities = await _pokemon.Abilities
      .Where(x => streamIds.Contains(x.StreamId))
      .ToDictionaryAsync(x => x.StreamId, x => x.AbilityId, cancellationToken);

    Dictionary<AbilitySlot, int> abilityIds = new(capacity: 3);
    abilityIds[AbilitySlot.Primary] = FindAbilityId(foundAbilities, abilities.PrimaryId);
    if (abilities.SecondaryId.HasValue)
    {
      abilityIds[AbilitySlot.Secondary] = FindAbilityId(foundAbilities, abilities.SecondaryId.Value);
    }
    if (abilities.HiddenId.HasValue)
    {
      abilityIds[AbilitySlot.Hidden] = FindAbilityId(foundAbilities, abilities.HiddenId.Value);
    }
    return abilityIds.AsReadOnly();
  }
  private static int FindAbilityId(Dictionary<string, int> abilities, AbilityId streamId)
  {
    if (abilities.TryGetValue(streamId.Value, out int abilityId))
    {
      return abilityId;
    }
    throw new InvalidOperationException($"The ability entity 'StreamId={streamId}' was not found.");
  }

  private async Task<IReadOnlyDictionary<FormSpriteKind, int>> FindAssetIdsAsync(FormSprites sprites, CancellationToken cancellationToken)
  {
    HashSet<string> streamIds = new(capacity: 4);
    streamIds.Add(sprites.DefaultId.Value);
    if (sprites.ShinyId.HasValue)
    {
      streamIds.Add(sprites.ShinyId.Value.Value);
    }
    if (sprites.FemaleId.HasValue)
    {
      streamIds.Add(sprites.FemaleId.Value.Value);
    }
    if (sprites.FemaleShinyId.HasValue)
    {
      streamIds.Add(sprites.FemaleShinyId.Value.Value);
    }
    Dictionary<string, int> assets = await _pokemon.Assets
      .Where(x => streamIds.Contains(x.StreamId))
      .ToDictionaryAsync(x => x.StreamId, x => x.AssetId, cancellationToken);

    Dictionary<FormSpriteKind, int> assetIds = new(capacity: 4);
    assetIds[FormSpriteKind.Default] = FindAssetId(assets, sprites.DefaultId);
    if (sprites.ShinyId.HasValue)
    {
      assetIds[FormSpriteKind.Shiny] = FindAssetId(assets, sprites.ShinyId.Value);
    }
    if (sprites.FemaleId.HasValue)
    {
      assetIds[FormSpriteKind.Female] = FindAssetId(assets, sprites.FemaleId.Value);
    }
    if (sprites.FemaleShinyId.HasValue)
    {
      assetIds[FormSpriteKind.FemaleShiny] = FindAssetId(assets, sprites.FemaleShinyId.Value);
    }
    return assetIds.AsReadOnly();
  }
  private static int FindAssetId(Dictionary<string, int> assets, AssetId streamId)
  {
    if (assets.TryGetValue(streamId.Value, out int assetId))
    {
      return assetId;
    }
    throw new InvalidOperationException($"The asset entity 'StreamId={streamId}' was not found.");
  }
}
