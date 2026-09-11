using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;

namespace PokeGame.Infrastructure.Entities;

internal class PokemonEntity : AggregateEntity
{
  public int PokemonId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public SpeciesEntity? Species { get; private set; }
  public int SpeciesId { get; private set; }
  public VarietyEntity? Variety { get; private set; }
  public int VarietyId { get; private set; }
  public FormEntity? Form { get; private set; }
  public int FormId { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Nickname { get; private set; }

  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public Gender? Gender { get; private set; }
  public bool IsShiny { get; private set; }
  public PokemonType TeraType { get; private set; }
  public AbilitySlot AbilitySlot { get; private set; }
  public byte Size { get; private set; }
  public string Nature { get; private set; } = string.Empty;

  public byte EggCycles { get; private set; }
  public GrowthRate GrowthRate { get; private set; }
  public int Experience { get; private set; }
  public int Level { get; private set; }

  public string? SkillRanks { get; private set; }

  public byte BaseHP { get; private set; }
  public byte BaseAttack { get; private set; }
  public byte BaseDefense { get; private set; }
  public byte BaseSpecialAttack { get; private set; }
  public byte BaseSpecialDefense { get; private set; }
  public byte BaseSpeed { get; private set; }

  public byte IndividualHP { get; private set; }
  public byte IndividualAttack { get; private set; }
  public byte IndividualDefense { get; private set; }
  public byte IndividualSpecialAttack { get; private set; }
  public byte IndividualSpecialDefense { get; private set; }
  public byte IndividualSpeed { get; private set; }

  public int Vitality { get; private set; }
  public int Stamina { get; private set; }
  public StatusCondition? Condition { get; private set; }
  public byte Friendship { get; private set; }

  public PokemonCharacteristic Characteristic { get; private set; }

  public ItemEntity? HeldItem { get; private set; }
  public int? HeldItemId { get; private set; }

  public AssetEntity? Sprite { get; private set; }
  public int? SpriteId { get; private set; }

  public TrainerEntity? OriginalTrainer { get; private set; }
  public int? OriginalTrainerId { get; private set; }
  public OwnershipEvent? OwnershipEvent { get; private set; }
  public TrainerEntity? CurrentTrainer { get; private set; }
  public int? CurrentTrainerId { get; private set; }
  public ItemEntity? PokeBall { get; private set; }
  public int? PokeBallId { get; private set; }
  public int? MetLevel { get; private set; }
  public string? MetAt { get; private set; }
  public DateTime? MetOn { get; private set; }

  public PokemonEntity(int worldId, int speciesId, int varietyId, int formId, PokemonCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new PokemonId(@event.StreamId).EntityId;

    SpeciesId = speciesId;
    VarietyId = varietyId;
    FormId = formId;

    Key = @event.Key.Value;

    Gender = @event.Gender;
    IsShiny = @event.IsShiny;
    TeraType = @event.TeraType;
    AbilitySlot = @event.AbilitySlot;
    Size = @event.Size.Scale;
    Nature = @event.Nature.Name;

    EggCycles = @event.EggCycles;
    GrowthRate = @event.GrowthRate;
    Experience = @event.Experience;
    Level = ExperienceTable.GetLevel(GrowthRate, Experience);

    BaseHP = @event.BaseStatistics.HP;
    BaseAttack = @event.BaseStatistics.Attack;
    BaseDefense = @event.BaseStatistics.Defense;
    BaseSpecialAttack = @event.BaseStatistics.SpecialAttack;
    BaseSpecialDefense = @event.BaseStatistics.SpecialDefense;
    BaseSpeed = @event.BaseStatistics.Speed;

    IndividualHP = @event.IndividualValues.HP;
    IndividualAttack = @event.IndividualValues.Attack;
    IndividualDefense = @event.IndividualValues.Defense;
    IndividualSpecialAttack = @event.IndividualValues.SpecialAttack;
    IndividualSpecialDefense = @event.IndividualValues.SpecialDefense;
    IndividualSpeed = @event.IndividualValues.Speed;

    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    Friendship = @event.Friendship.Value;

    Characteristic = @event.Characteristic;
  }

  private PokemonEntity()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Form is not null)
    {
      actorIds.AddRange(Form.GetActorIds());
    }
    if (HeldItem is not null)
    {
      actorIds.AddRange(HeldItem.GetActorIds());
    }
    if (Sprite is not null)
    {
      actorIds.AddRange(Sprite.GetActorIds());
    }
    if (OriginalTrainer is not null)
    {
      actorIds.AddRange(OriginalTrainer.GetActorIds());
    }
    if (CurrentTrainer is not null)
    {
      actorIds.AddRange(CurrentTrainer.GetActorIds());
    }
    if (PokeBall is not null)
    {
      actorIds.AddRange(PokeBall.GetActorIds());
    }
    return actorIds;
  }

  public IReadOnlyDictionary<PokemonSkill, byte> GetSkillRanks()
  {
    if (SkillRanks is null)
    {
      return new Dictionary<PokemonSkill, byte>();
    }

    string[] values = SkillRanks.Split('|');
    Dictionary<PokemonSkill, byte> skillRanks = new(capacity: values.Length);
    foreach (string value in values)
    {
      string[] pair = value.Split(':');
      if (pair.Length == 2 && Enum.TryParse(pair[0], out PokemonSkill skill) && Enum.IsDefined(skill) && byte.TryParse(pair[1], out byte rank))
      {
        skillRanks[skill] = rank;
      }
    }
    return skillRanks.AsReadOnly();
  }

  public BaseStatisticsDto GetBaseStatistics() => new(BaseHP, BaseAttack, BaseDefense, BaseSpecialAttack, BaseSpecialDefense, BaseSpeed);
  public IndividualValuesDto GetIndividualValues() => new(IndividualHP, IndividualAttack, IndividualDefense, IndividualSpecialAttack, IndividualSpecialDefense, IndividualSpeed);

  public void ChangeForm(int formId, PokemonFormChanged @event)
  {
    Update(@event);

    FormId = formId;

    BaseHP = @event.BaseStatistics.HP;
    BaseAttack = @event.BaseStatistics.Attack;
    BaseDefense = @event.BaseStatistics.Defense;
    BaseSpecialAttack = @event.BaseStatistics.SpecialAttack;
    BaseSpecialDefense = @event.BaseStatistics.SpecialDefense;
    BaseSpeed = @event.BaseStatistics.Speed;

    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
  }

  public void Receive(int trainerId, int pokeBallId, PokemonReceived @event)
  {
    Update(@event);

    if (!OriginalTrainerId.HasValue && EggCycles < 1)
    {
      OriginalTrainerId = trainerId;
    }

    OwnershipEvent = Core.Pokemon.OwnershipEvent.Received;
    CurrentTrainerId = trainerId;
    PokeBallId = pokeBallId;
    MetLevel = @event.Level.Value;
    MetAt = @event.Location.Value;
    MetOn = @event.OccurredOn.AsUniversalTime();
  }

  public void SetDetails(PokemonDetailsChanged @event)
  {
    Update(@event);

    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetHeldItem(int? heldItemId, PokemonHeldItemChanged @event)
  {
    Update(@event);

    HeldItemId = heldItemId;
  }

  public void SetKey(PokemonKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetNickname(PokemonNicknameChanged @event)
  {
    Update(@event);

    Nickname = @event.Nickname?.Value;
  }

  public void SetSprite(int? spriteId, PokemonSpriteChanged @event)
  {
    Update(@event);

    SpriteId = spriteId;
  }

  public void SetStatus(PokemonStatusChanged @event)
  {
    Update(@event);

    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    Condition = @event.Condition;
    Friendship = @event.Friendship.Value;
  }

  public override string ToString() => $"{Nickname ?? Key} | {base.ToString()}";
}
