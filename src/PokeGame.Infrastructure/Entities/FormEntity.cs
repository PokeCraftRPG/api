using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Events;

namespace PokeGame.Infrastructure.Entities;

internal class FormEntity : AggregateEntity
{
  public int FormId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public VarietyEntity? Variety { get; private set; }
  public int VarietyId { get; private set; }
  public FormCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public PokemonType PrimaryType { get; private set; }
  public PokemonType? SecondaryType { get; private set; }

  public int BaseHP { get; private set; }
  public int BaseAttack { get; private set; }
  public int BaseDefense { get; private set; }
  public int BaseSpecialAttack { get; private set; }
  public int BaseSpecialDefense { get; private set; }
  public int BaseSpeed { get; private set; }

  public int YieldExperience { get; private set; }
  public int YieldHP { get; private set; }
  public int YieldAttack { get; private set; }
  public int YieldDefense { get; private set; }
  public int YieldSpecialAttack { get; private set; }
  public int YieldSpecialDefense { get; private set; }
  public int YieldSpeed { get; private set; }

  public int? Height { get; private set; }
  public int? Weight { get; private set; }

  public List<FormAbilityEntity> Abilities { get; private set; } = [];
  public List<FormSpriteEntity> Sprites { get; private set; } = [];

  public FormEntity(int worldId, int varietyId, IReadOnlyDictionary<AbilitySlot, int> abilityIds, FormCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new FormId(@event.StreamId).EntityId;

    VarietyId = varietyId;
    Category = @event.Category;

    Key = @event.Key.Value;

    SetTypes(@event.Types);
    SetAbilities(abilityIds);
    SetStatistics(@event.Statistics);
    SetYield(@event.Yield);
    Height = @event.Size.Height;
    Weight = @event.Size.Weight;
  }

  private FormEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Variety is not null)
    {
      actorIds.AddRange(Variety.GetActorIds());
    }
    foreach (FormAbilityEntity entity in Abilities)
    {
      if (entity.Ability is not null)
      {
        actorIds.AddRange(entity.Ability.GetActorIds());
      }
    }
    foreach (FormSpriteEntity entity in Sprites)
    {
      if (entity.Asset is not null)
      {
        actorIds.AddRange(entity.Asset.GetActorIds());
      }
    }
    return actorIds;
  }

  public void SetCharacteristics(IReadOnlyDictionary<AbilitySlot, int> abilityIds, FormCharacteristicsChanged @event)
  {
    Update(@event);

    SetTypes(@event.Types);
    SetAbilities(abilityIds);
    SetStatistics(@event.BaseStatistics);
    SetYield(@event.Yield);
    Height = @event.Size.Height;
    Weight = @event.Size.Weight;
  }

  public void SetDetails(FormDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(FormKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetSprites(IReadOnlyDictionary<FormSpriteKind, int> assetIds, FormSpritesChanged @event)
  {
    Update(@event);

    Sprites.RemoveAll(x => !assetIds.ContainsKey(x.Kind));

    Dictionary<FormSpriteKind, FormSpriteEntity> sprites = Sprites.ToDictionary(x => x.Kind, x => x);
    foreach (KeyValuePair<FormSpriteKind, int> assetId in assetIds)
    {
      if (sprites.TryGetValue(assetId.Key, out FormSpriteEntity? sprite))
      {
        sprite.AssetId = assetId.Value;
      }
      else
      {
        sprite = new FormSpriteEntity(this, assetId.Key, assetId.Value);
        Sprites.Add(sprite);
      }
    }
  }

  private void SetStatistics(BaseStatistics statistics)
  {
    BaseHP = statistics.HP;
    BaseAttack = statistics.Attack;
    BaseDefense = statistics.Defense;
    BaseSpecialAttack = statistics.SpecialAttack;
    BaseSpecialDefense = statistics.SpecialDefense;
    BaseSpeed = statistics.Speed;
  }

  private void SetTypes(FormTypes types)
  {
    PrimaryType = types.Primary;
    SecondaryType = types.Secondary;
  }

  private void SetYield(FormYield yield)
  {
    YieldExperience = yield.Experience;
    YieldHP = yield.HP;
    YieldAttack = yield.Attack;
    YieldDefense = yield.Defense;
    YieldSpecialAttack = yield.SpecialAttack;
    YieldSpecialDefense = yield.SpecialDefense;
    YieldSpeed = yield.Speed;
  }

  private void SetAbilities(IReadOnlyDictionary<AbilitySlot, int> abilityIds)
  {
    Abilities.RemoveAll(x => !abilityIds.ContainsKey(x.Slot));

    Dictionary<AbilitySlot, FormAbilityEntity> abilities = Abilities.ToDictionary(x => x.Slot, x => x);
    foreach (KeyValuePair<AbilitySlot, int> abilityId in abilityIds)
    {
      if (abilities.TryGetValue(abilityId.Key, out FormAbilityEntity? ability))
      {
        ability.AbilityId = abilityId.Value;
      }
      else
      {
        ability = new FormAbilityEntity(this, abilityId.Key, abilityId.Value);
        Abilities.Add(ability);
      }
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
