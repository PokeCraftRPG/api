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

  public FormEntity(int worldId, int varietyId, FormCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    VarietyId = varietyId;
    Category = @event.Category;

    Key = @event.Key.Value;

    SetTypes(@event.Types);
    // TODO(fpion): SetAbilities(primaryAbilityId, secondaryAbilityId, hiddenAbilityId);
    SetStatistics(@event.Statistics);
    SetYield(@event.Yield);
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
    // TODO(fpion): Abilities
    // TODO(fpion): Sprites
    return actorIds;
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

  public void SetMechanics(FormMechanicsChanged @event)
  {
    Update(@event);

    SetTypes(@event.Types);
    // TODO(fpion): SetAbilities(primaryAbilityId, secondaryAbilityId, hiddenAbilityId);
    SetStatistics(@event.BaseStatistics);
    SetYield(@event.Yield);
  }

  public void SetTraits(FormTraitsChanged @event)
  {
    Update(@event);

    Height = @event.Size?.Height;
    Weight = @event.Size?.Weight;

    // TODO(fpion): Sprites
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

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
