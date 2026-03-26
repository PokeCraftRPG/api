using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
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
  public bool IsDefault { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public bool IsBattleOnly { get; private set; }
  public bool IsMega { get; private set; }

  public int Height { get; private set; }
  public int Weight { get; private set; }

  public PokemonType PrimaryType { get; private set; }
  public PokemonType? SecondaryType { get; private set; }

  public byte BaseHP { get; private set; }
  public byte BaseAttack { get; private set; }
  public byte BaseDefense { get; private set; }
  public byte BaseSpecialAttack { get; private set; }
  public byte BaseSpecialDefense { get; private set; }
  public byte BaseSpeed { get; private set; }

  public int YieldExperience { get; private set; }
  public int YieldHP { get; private set; }
  public int YieldAttack { get; private set; }
  public int YieldDefense { get; private set; }
  public int YieldSpecialAttack { get; private set; }
  public int YieldSpecialDefense { get; private set; }
  public int YieldSpeed { get; private set; }

  public string SpriteDefault { get; private set; } = string.Empty;
  public string SpriteShiny { get; private set; } = string.Empty;
  public string? SpriteAlternative { get; private set; }
  public string? SpriteAlternativeShiny { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public List<FormAbilityEntity> Abilities { get; private set; } = [];

  public FormEntity(VarietyEntity variety, FormCreated @event) : base(@event)
  {
    Id = new FormId(@event.StreamId).EntityId;

    World = variety.World;
    WorldId = variety.WorldId;

    Variety = variety;
    VarietyId = variety.VarietyId;
    IsDefault = @event.IsDefault;

    Key = @event.Key.Value;

    Height = @event.Height.Value;
    Weight = @event.Weight.Value;

    SetTypes(@event.Types);
    SetBaseStatistics(@event.BaseStatistics);
    SetYield(@event.Yield);
    SetSprites(@event.Sprites);
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
    foreach (FormAbilityEntity formAbility in Abilities)
    {
      if (formAbility.Ability is not null)
      {
        actorIds.AddRange(formAbility.Ability.GetActorIds());
      }
    }
    return actorIds;
  }

  public void SetDefault(FormDefaultChanged @event)
  {
    base.Update(@event);

    IsDefault = @event.IsDefault;
  }

  public void SetKey(FormKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(FormUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }

    if (@event.IsBattleOnly.HasValue)
    {
      IsBattleOnly = @event.IsBattleOnly.Value;
    }
    if (@event.IsMega.HasValue)
    {
      IsMega = @event.IsMega.Value;
    }

    if (@event.Height is not null)
    {
      Height = @event.Height.Value;
    }
    if (@event.Weight is not null)
    {
      Weight = @event.Weight.Value;
    }

    if (@event.Types is not null)
    {
      SetTypes(@event.Types);
    }
    if (@event.BaseStatistics is not null)
    {
      SetBaseStatistics(@event.BaseStatistics);
    }
    if (@event.Yield is not null)
    {
      SetYield(@event.Yield);
    }
    if (@event.Sprites is not null)
    {
      SetSprites(@event.Sprites);
    }

    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  private void SetBaseStatistics(BaseStatistics baseStatistics)
  {
    BaseHP = baseStatistics.HP;
    BaseAttack = baseStatistics.Attack;
    BaseDefense = baseStatistics.Defense;
    BaseSpecialAttack = baseStatistics.SpecialAttack;
    BaseSpecialDefense = baseStatistics.SpecialDefense;
    BaseSpeed = baseStatistics.Speed;
  }

  private void SetSprites(Sprites sprites)
  {
    SpriteDefault = sprites.Default.Value;
    SpriteShiny = sprites.Shiny.Value;
    SpriteAlternative = sprites.Alternative?.Value;
    SpriteAlternativeShiny = sprites.AlternativeShiny?.Value;
  }

  private void SetTypes(Types types)
  {
    PrimaryType = types.Primary;
    SecondaryType = types.Secondary;
  }

  private void SetYield(Yield yield)
  {
    YieldExperience = yield.Experience;
    YieldHP = yield.HP;
    YieldAttack = yield.Attack;
    YieldDefense = yield.Defense;
    YieldSpecialAttack = yield.SpecialAttack;
    YieldSpecialDefense = yield.SpecialDefense;
    YieldSpeed = yield.Speed;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
