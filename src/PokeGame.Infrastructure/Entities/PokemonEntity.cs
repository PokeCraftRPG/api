using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;

namespace PokeGame.Infrastructure.Entities;

internal class PokemonEntity : AggregateEntity
{
  public int PokemonId { get; set; }

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
  public string? Name { get; private set; }
  public PokemonGender? Gender { get; private set; }
  public bool IsShiny { get; private set; }

  public PokemonType TeraType { get; private set; }
  public byte Height { get; private set; }
  public byte Weight { get; private set; }
  public PokemonSizeCategory Size { get; private set; }
  public AbilitySlot AbilitySlot { get; private set; }
  public string Nature { get; private set; } = string.Empty;

  public byte? EggCycles { get; private set; }
  public bool IsEgg
  {
    get => EggCycles.HasValue;
    private set { }
  }
  public GrowthRate GrowthRate { get; private set; }
  public int Level { get; private set; }
  public int Experience { get; private set; }
  public int MaximumExperience { get; private set; }
  public int ToNextLevel { get; private set; }

  public string Statistics { get; private set; } = string.Empty;
  public int Vitality { get; private set; }
  public int Stamina { get; private set; }
  public StatusCondition? StatusCondition { get; private set; }
  public byte Friendship { get; private set; }

  public string Characteristic { get; private set; } = string.Empty;

  public ItemEntity? HeldItem { get; private set; }
  public int? HeldItemId { get; private set; }

  public TrainerEntity? OriginalTrainer { get; private set; }
  public int? OriginalTrainerId { get; private set; }

  public OwnershipKind? OwnershipKind { get; private set; }
  public TrainerEntity? CurrentTrainer { get; private set; }
  public int? CurrentTrainerId { get; private set; }
  public ItemEntity? PokeBall { get; private set; }
  public int? PokeBallId { get; private set; }
  public int? MetAtLevel { get; private set; }
  public string? MetAtLocation { get; private set; }
  public DateTime? MetOn { get; private set; }
  public int? Position { get; private set; }
  public int? Box { get; private set; }

  public string? Sprite { get; private set; }
  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public PokemonEntity(FormEntity form, PokemonCreated @event) : base(@event)
  {
    VarietyEntity variety = form.Variety ?? throw new ArgumentException("The variety is required.", nameof(form));
    SpeciesEntity species = variety.Species ?? throw new ArgumentException("The species is required.", nameof(form));
    WorldEntity world = species.World ?? throw new ArgumentException("The world is required.", nameof(form));

    World = world;
    WorldId = world.WorldId;
    Id = new PokemonId(@event.StreamId).EntityId;

    Species = species;
    SpeciesId = species.SpeciesId;
    Variety = variety;
    VarietyId = variety.VarietyId;
    Form = form;
    FormId = form.FormId;

    Key = @event.Key.Value;
    Gender = @event.Gender;
    IsShiny = @event.IsShiny;

    TeraType = @event.TeraType;
    Height = @event.Size.Height;
    Weight = @event.Size.Weight;
    Size = @event.Size.Category;
    AbilitySlot = @event.AbilitySlot;
    Nature = @event.Nature.Name;

    EggCycles = @event.EggCycles?.Value;
    GrowthRate = @event.GrowthRate;
    Experience = @event.Experience;
    Level = ExperienceTable.Instance.GetLevel(GrowthRate, Experience);
    MaximumExperience = ExperienceTable.Instance.GetMaximumExperience(GrowthRate, Level);
    ToNextLevel = Math.Max(MaximumExperience - Experience, 0);

    SetStatistics(@event.BaseStatistics, @event.IndividualValues, @event.EffortValues);
    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    Friendship = @event.Friendship.Value;

    Characteristic = PokemonCharacteristics.Instance.Find(@event.IndividualValues, @event.Size).Text;
  }

  private PokemonEntity()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Species is not null)
    {
      actorIds.AddRange(Species.GetActorIds());
    }
    if (Variety is not null)
    {
      actorIds.AddRange(Variety.GetActorIds());
    }
    if (Form is not null)
    {
      actorIds.AddRange(Form.GetActorIds());
    }
    if (HeldItem is not null)
    {
      actorIds.AddRange(HeldItem.GetActorIds());
    }
    return actorIds;
  }

  public void Catch(TrainerEntity trainer, ItemEntity pokeBall, PokemonCaught @event)
  {
    base.Update(@event);

    SetOwnership(Core.Pokemon.OwnershipKind.Caught, trainer, pokeBall, @event.Level, @event.Location, @event.MetOn);
  }

  public void ChangeForm(FormEntity form, PokemonFormChanged @event)
  {
    base.Update(@event);

    Form = form;
    FormId = form.FormId;

    SetStatistics(@event.BaseStatistics);
  }

  public void Deposit(PokemonDeposited @event)
  {
    base.Update(@event);

    Position = @event.Slot.Position;
    Box = @event.Slot.Box;
  }

  public void Nickname(PokemonNicknamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void Move(PokemonMoved @event)
  {
    base.Update(@event);

    Position = @event.Slot.Position;
    Box = @event.Slot.Box;
  }

  public void Receive(TrainerEntity trainer, ItemEntity pokeBall, PokemonReceived @event)
  {
    base.Update(@event);

    SetOwnership(Core.Pokemon.OwnershipKind.Received, trainer, pokeBall, @event.Level, @event.Location, @event.MetOn);
  }

  public void Release(PokemonReleased @event)
  {
    base.Update(@event);

    OwnershipKind = null;
    CurrentTrainer = null;
    CurrentTrainerId = null;
    PokeBall = null;
    PokeBallId = null;
    MetAtLevel = null;
    MetAtLocation = null;
    MetOn = null;
    Position = null;
    Box = null;
  }

  public void SetHeldItem(ItemEntity? item, PokemonHeldItemChanged @event)
  {
    base.Update(@event);

    HeldItem = item;
    HeldItemId = item?.ItemId;
  }

  public void SetKey(PokemonKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(PokemonUpdated @event)
  {
    base.Update(@event);

    if (@event.Sprite is not null)
    {
      Sprite = @event.Sprite.Value?.Value;
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

  public void Withdraw(PokemonWithdrawn @event)
  {
    base.Update(@event);

    Position = @event.Slot.Position;
    Box = @event.Slot.Box;
  }

  public PokemonStatisticsModel GetStatistics()
  {
    string[] properties = Statistics.Split('|');
    byte[] baseStatistics = properties[0].Split(',').Select(byte.Parse).ToArray();
    byte[] individualValues = properties[1].Split(',').Select(byte.Parse).ToArray();
    byte[] effortValues = properties[2].Split(',').Select(byte.Parse).ToArray();
    int[] values = properties[3].Split(',').Select(int.Parse).ToArray();
    return new PokemonStatisticsModel(
      new PokemonStatisticModel(baseStatistics[0], individualValues[0], effortValues[0], values[0]),
      new PokemonStatisticModel(baseStatistics[1], individualValues[1], effortValues[1], values[1]),
      new PokemonStatisticModel(baseStatistics[2], individualValues[2], effortValues[2], values[2]),
      new PokemonStatisticModel(baseStatistics[3], individualValues[3], effortValues[3], values[3]),
      new PokemonStatisticModel(baseStatistics[4], individualValues[4], effortValues[4], values[4]),
      new PokemonStatisticModel(baseStatistics[5], individualValues[5], effortValues[5], values[5]));
  }
  private void SetStatistics(IBaseStatistics? baseStatistics = null, IIndividualValues? individualValues = null, IEffortValues? effortValues = null)
  {
    if (baseStatistics is null || individualValues is null || effortValues is null)
    {
      PokemonStatisticsModel statistics = GetStatistics();
      baseStatistics ??= new BaseStatistics(statistics.HP.Base, statistics.Attack.Base, statistics.Defense.Base,
        statistics.SpecialAttack.Base, statistics.SpecialDefense.Base, statistics.Speed.Base);
      individualValues ??= new IndividualValues(statistics.HP.IndividualValue, statistics.Attack.IndividualValue, statistics.Defense.IndividualValue,
        statistics.SpecialAttack.IndividualValue, statistics.SpecialDefense.IndividualValue, statistics.Speed.IndividualValue);
      effortValues ??= new EffortValues(statistics.HP.EffortValue, statistics.Attack.EffortValue, statistics.Defense.EffortValue,
        statistics.SpecialAttack.EffortValue, statistics.SpecialDefense.EffortValue, statistics.Speed.EffortValue);
    }

    PokemonStatistics values = new(baseStatistics, individualValues, effortValues, Level, PokemonNatures.Instance.Find(Nature));
    Statistics = string.Join('|',
      string.Join(',', baseStatistics.HP, baseStatistics.Attack, baseStatistics.Defense, baseStatistics.SpecialAttack, baseStatistics.SpecialDefense, baseStatistics.Speed),
      string.Join(',', individualValues.HP, individualValues.Attack, individualValues.Defense, individualValues.SpecialAttack, individualValues.SpecialDefense, individualValues.Speed),
      string.Join(',', effortValues.HP, effortValues.Attack, effortValues.Defense, effortValues.SpecialAttack, effortValues.SpecialDefense, effortValues.Speed),
      string.Join(',', values.HP, values.Attack, values.Defense, values.SpecialAttack, values.SpecialDefense, values.Speed));

    Vitality = Math.Min(Vitality, values.HP);
    Stamina = Math.Min(Stamina, values.HP);
  }

  private void SetOwnership(OwnershipKind kind, TrainerEntity trainer, ItemEntity pokeBall, Level level, Location location, DateTime metOn)
  {
    if (!OriginalTrainerId.HasValue)
    {
      OriginalTrainer = trainer;
      OriginalTrainerId = trainer.TrainerId;
    }

    OwnershipKind = kind;
    CurrentTrainer = trainer;
    CurrentTrainerId = trainer.TrainerId;
    PokeBall = pokeBall;
    PokeBallId = pokeBall.ItemId;
    MetAtLevel = level.Value;
    MetAtLocation = location.Value;
    MetOn = metOn.AsUniversalTime();
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
