using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public sealed class Specimen : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Specimen";

  public new PokemonId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public SpeciesId SpeciesId { get; private set; }
  public VarietyId VarietyId { get; private set; }
  public FormId FormId { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Nickname { get; private set; }

  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Gender? Gender { get; private set; }
  public bool IsShiny { get; private set; }
  public PokemonType TeraType { get; private set; }
  public AbilitySlot AbilitySlot { get; private set; }
  private PokemonSize? _size = null;
  public PokemonSize Size => _size ?? throw new InvalidOperationException("The size was not initialized.");
  private PokemonNature? _nature = null;
  public PokemonNature Nature => _nature ?? throw new InvalidOperationException("The nature was not initialized.");

  public byte EggCycles { get; private set; }
  public bool IsEgg => EggCycles > 0;
  public GrowthRate GrowthRate { get; private set; }
  public int Experience { get; private set; }
  public int Level => ExperienceTable.GetLevel(GrowthRate, Experience);

  private readonly Dictionary<PokemonSkill, byte> _skillRanks = [];
  public IReadOnlyDictionary<PokemonSkill, byte> SkillRanks => _skillRanks.AsReadOnly();

  private BaseStatistics? _baseStatistics = null;
  public BaseStatistics BaseStatistics => _baseStatistics ?? throw new InvalidOperationException("The base statistics were not initialized.");
  public IndividualValues IndividualValues { get; private set; } = new();
  public EffortValues EffortValues => new(SkillRanks);

  public int Vitality { get; private set; }
  public int Stamina { get; private set; }
  public bool IsFainted => Vitality < 1;
  public StatusCondition? Condition { get; private set; }
  public Friendship Friendship { get; private set; } = new();

  public PokemonCharacteristic Characteristic { get; private set; }

  public ItemId? HeldItemId { get; private set; }

  public AssetId? SpriteId { get; private set; }

  public TrainerId? OriginalTrainerId { get; private set; }
  public PokemonOwnership? Ownership { get; private set; }

  public Specimen() : base()
  {
  }

  public Specimen(
    IPokemonRandomizer randomizer,
    PokemonId pokemonId,
    PokemonSpecies species,
    Variety variety,
    Form form,
    Key? key = null,
    Gender? gender = null,
    bool? isShiny = null,
    PokemonType? teraType = null,
    AbilitySlot? abilitySlot = null,
    PokemonSize? size = null,
    PokemonNature? nature = null,
    byte eggCycles = 0,
    int experience = 0,
    IndividualValues? individualValues = null,
    ActorId? actorId = null) : base(pokemonId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, species, nameof(species));
    if (eggCycles > species.Eggs.Cycles)
    {
      throw new InvalidEggCyclesException(this, species, eggCycles);
    }

    WorldMismatchException.ThrowIfMismatch(this, variety, nameof(variety));
    if (variety.SpeciesId != species.Id)
    {
      throw new ArgumentException($"The variety '{variety}' does not belong to the species '{species}'.", nameof(variety));
    }

    WorldMismatchException.ThrowIfMismatch(this, form, nameof(form));
    if (form.VarietyId != variety.Id)
    {
      throw new ArgumentException($"The form '{form}' does not belong to the variety '{variety}'.", nameof(form));
    }
    if (form.Category != FormCategory.Default && form.Category != FormCategory.Alternative)
    {
      throw new InvalidPokemonFormCategoryException(this, form);
    }

    ArgumentOutOfRangeException.ThrowIfNegative(experience, nameof(experience));
    if (eggCycles > 0 && experience > 0)
    {
      throw new InvalidOperationException("Egg cycles and experience cannot both be greater than zero.");
    }

    key ??= species.Key;
    gender = PokemonHelper.ResolveGender(randomizer, this, variety, gender);
    isShiny ??= randomizer.Shininess();
    teraType ??= randomizer.TeraType(form.Types);
    abilitySlot = PokemonHelper.ResolveAbilitySlot(randomizer, this, form, abilitySlot);
    size ??= randomizer.Size();
    nature ??= randomizer.Nature();
    individualValues ??= randomizer.IndividualValues();
    PokemonCharacteristic characteristic = randomizer.Characteristic(individualValues);

    int level = ExperienceTable.GetLevel(species.GrowthRate, experience);
    PokemonStatistics statistics = new(form.BaseStatistics, individualValues, EffortValues, level, nature);

    PokemonCreated @event = new(species.Id, variety.Id, form.Id, key, gender, isShiny.Value, teraType.Value, abilitySlot.Value, size, nature, eggCycles,
      species.GrowthRate, experience, form.BaseStatistics, individualValues, statistics.HP, statistics.HP, species.BaseFriendship, characteristic);
    Raise(@event, actorId);
  }
  private void Handle(PokemonCreated @event)
  {
    SpeciesId = @event.SpeciesId;
    VarietyId = @event.VarietyId;
    FormId = @event.FormId;

    _key = @event.Key;

    Gender = @event.Gender;
    IsShiny = @event.IsShiny;
    TeraType = @event.TeraType;
    AbilitySlot = @event.AbilitySlot;
    _size = @event.Size;
    _nature = @event.Nature;

    EggCycles = @event.EggCycles;
    GrowthRate = @event.GrowthRate;
    Experience = @event.Experience;

    _baseStatistics = @event.BaseStatistics;
    IndividualValues = @event.IndividualValues;

    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    Friendship = @event.Friendship;

    Characteristic = @event.Characteristic;
  }

  public void ChangeForm(Form form, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, form, nameof(form));
    if (form.VarietyId != VarietyId)
    {
      throw new InvalidPokemonFormException(this, form);
    }

    FormId formId = form.Id;
    if (!Equals(FormId, formId))
    {
      PokemonStatistics current = new(this);
      PokemonStatistics changed = new(form.BaseStatistics, IndividualValues, EffortValues, Level, Nature);
      int delta = changed.HP - current.HP;
      int vitality = Math.Clamp(Vitality + delta, 0, changed.HP);
      int stamina = Math.Clamp(Stamina + delta, 0, changed.HP);

      Raise(new PokemonFormChanged(formId, form.BaseStatistics, vitality, stamina), actorId);
    }
  }
  private void Handle(PokemonFormChanged @event)
  {
    FormId = @event.FormId;
    _baseStatistics = @event.BaseStatistics;
    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new PokemonDeleted(), actorId);
    }
  }

  public void Receive(Trainer trainer, Item pokeBall, Location location, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, trainer, nameof(trainer));
    WorldMismatchException.ThrowIfMismatch(this, pokeBall, nameof(pokeBall));

    if (pokeBall.Category != ItemCategory.PokeBall)
    {
      throw new InvalidItemCategoryException(pokeBall, ItemCategory.PokeBall, nameof(Ownership.PokeBallId));
    }

    TrainerId trainerId = trainer.Id;
    ItemId pokeBallId = pokeBall.Id;

    if (Ownership is not null)
    {
      if (Ownership.TrainerId == trainerId)
      {
        throw new PokemonAlreadyOwnedException(this);
      }
      if (Ownership.PokeBallId != pokeBallId)
      {
        throw new ImmutablePropertyException<Guid>(this, Ownership.PokeBallId.EntityId, pokeBallId.EntityId, nameof(Ownership.PokeBallId));
      }
    }

    Raise(new PokemonReceived(trainerId, pokeBallId, new Level(Level), location), actorId);
  }
  private void Handle(PokemonReceived @event)
  {
    if (!OriginalTrainerId.HasValue && !IsEgg)
    {
      OriginalTrainerId = @event.TrainerId;
    }

    Ownership = new PokemonOwnership(OwnershipEvent.Received, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.OccurredOn);
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new PokemonDetailsChanged(summary, content), actorId);
    }
  }
  private void Handle(PokemonDetailsChanged @event)
  {
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetHeldItem(Item? heldItem, ActorId? actorId = null)
  {
    if (heldItem is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, heldItem, nameof(heldItem));
    }

    ItemId? heldItemId = heldItem?.Id;
    if (!Equals(HeldItemId, heldItemId))
    {
      Raise(new PokemonHeldItemChanged(heldItemId), actorId);
    }
  }
  private void Handle(PokemonHeldItemChanged @event)
  {
    HeldItemId = @event.HeldItemId;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new PokemonKeyChanged(key), actorId);
    }
  }
  private void Handle(PokemonKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetNickname(Name? nickname, ActorId? actorId = null)
  {
    if (!Equals(Nickname, nickname))
    {
      Raise(new PokemonNicknameChanged(nickname), actorId);
    }
  }
  private void Handle(PokemonNicknameChanged @event)
  {
    Nickname = @event.Nickname;
  }

  public void SetSprite(Asset? sprite, ActorId? actorId = null)
  {
    if (sprite is not null)
    {
      WorldMismatchException.ThrowIfMismatch(this, sprite, nameof(sprite));
      InvalidAssetKindException.ThrowIfNotValid(sprite, AssetKind.Image, nameof(SpriteId));
    }

    AssetId? spriteId = sprite?.Id;
    if (!Equals(SpriteId, spriteId))
    {
      Raise(new PokemonSpriteChanged(spriteId), actorId);
    }
  }
  private void Handle(PokemonSpriteChanged @event)
  {
    SpriteId = @event.SpriteId;
  }

  public void SetStatus(int vitality, int stamina, StatusCondition? condition, Friendship friendship, ActorId? actorId = null)
  {
    PokemonStatistics statistics = new(this);

    ArgumentOutOfRangeException.ThrowIfNegative(vitality, nameof(vitality));
    if (vitality > statistics.HP)
    {
      throw new ConstitutionOutOfRangeException(this, vitality, nameof(Vitality));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(stamina, nameof(stamina));
    if (stamina > statistics.HP)
    {
      throw new ConstitutionOutOfRangeException(this, stamina, nameof(Stamina));
    }

    if (condition.HasValue && !Enum.IsDefined(condition.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(condition));
    }

    if (!Equals(Vitality, vitality) || !Equals(Stamina, stamina) || !Equals(Condition, condition) || !Equals(Friendship, friendship))
    {
      Raise(new PokemonStatusChanged(vitality, stamina, condition, friendship), actorId);
    }
  }
  private void Handle(PokemonStatusChanged @event)
  {
    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    Condition = @event.Condition;
    Friendship = @event.Friendship;
  }

  public override string ToString() => $"{Nickname?.Value ?? Key.Value} | {base.ToString()}";
}
