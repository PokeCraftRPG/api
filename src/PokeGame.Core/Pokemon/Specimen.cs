using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public class Specimen : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Specimen";
  public const int MinimumVitality = 0;
  public const int MaximumVitality = 999;
  public const int MinimumStamina = 0;
  public const int MaximumStamina = 999;

  private PokemonUpdated _updated = new();
  private bool HasUpdates => _updated.Sprite is not null || _updated.Url is not null || _updated.Notes is not null;

  public new SpecimenId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public SpeciesId SpeciesId { get; private set; }
  public VarietyId VarietyId { get; private set; }
  public FormId FormId { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The specimen was not initialized.");
  public Name? Name { get; private set; }
  public PokemonGender? Gender { get; private set; }
  public bool IsShiny { get; private set; }

  public PokemonType TeraType { get; private set; }
  private PokemonSize? _size = null;
  public PokemonSize Size => _size ?? throw new InvalidOperationException("The specimen was not initialized.");
  public AbilitySlot AbilitySlot { get; private set; }
  private PokemonNature? _nature = null;
  public PokemonNature Nature => _nature ?? throw new InvalidOperationException("The specimen was not initialized.");

  public EggCycles? EggCycles { get; private set; }
  public bool IsEgg => EggCycles is not null;
  public GrowthRate GrowthRate { get; private set; }
  public int Experience { get; private set; }
  public int Level => ExperienceTable.Instance.GetLevel(GrowthRate, Experience);

  private BaseStatistics? _baseStatistics = null;
  public BaseStatistics BaseStatistics => _baseStatistics ?? throw new InvalidOperationException("The Pokémon has not been initialized.");
  private IndividualValues? _individualValues = null;
  public IndividualValues IndividualValues => _individualValues ?? throw new InvalidOperationException("The Pokémon has not been initialized.");
  private EffortValues? _effortValues = null;
  public EffortValues EffortValues => _effortValues ?? throw new InvalidOperationException("The Pokémon has not been initialized.");
  public PokemonStatistics Statistics => new(this);
  public int Vitality { get; private set; }
  public bool HasFainted => Vitality < 1;
  public int Stamina { get; private set; }
  public bool IsUnconscious => Stamina < 1;
  public StatusCondition? StatusCondition { get; private set; }
  private Friendship? _friendship = null;
  public Friendship Friendship => _friendship ?? throw new InvalidOperationException("The Pokémon has not been initialized.");

  public PokemonCharacteristic Characteristic => PokemonCharacteristics.Instance.Find(IndividualValues, Size);

  public ItemId? HeldItemId { get; private set; }

  private Url? _sprite = null;
  public Url? Sprite
  {
    get => _sprite;
    set
    {
      if (_sprite != value)
      {
        _sprite = value;
        _updated.Sprite = new Optional<Url>(value);
      }
    }
  }
  private Url? _url = null;
  public Url? Url
  {
    get => _url;
    set
    {
      if (_url != value)
      {
        _url = value;
        _updated.Url = new Optional<Url>(value);
      }
    }
  }
  private Notes? _notes = null;
  public Notes? Notes
  {
    get => _notes;
    set
    {
      if (_notes != value)
      {
        _notes = value;
        _updated.Notes = new Optional<Notes>(value);
      }
    }
  }

  public long SizeBytes => Key.Size + (Name?.Size ?? 0) + (Sprite?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Specimen() : base()
  {
  }

  public Specimen(
    World world,
    SpeciesAggregate species,
    Variety variety,
    Form form,
    Slug? key,
    PokemonGender? gender,
    bool isShiny,
    PokemonType? teraType,
    PokemonSize size,
    AbilitySlot abilitySlot,
    PokemonNature nature,
    EggCycles? eggCycles,
    int experience,
    IndividualValues individualValues,
    EffortValues? effortValues,
    int? vitality,
    int? stamina,
    Friendship? friendship) : this(species, variety, form, key, gender, isShiny, teraType, size, abilitySlot, nature, eggCycles, experience, individualValues, effortValues, vitality, stamina, friendship, world.OwnerId, SpecimenId.NewId(world.Id))
  {
  }

  public Specimen(
    SpeciesAggregate species,
    Variety variety,
    Form form,
    Slug? key,
    PokemonGender? gender,
    bool isShiny,
    PokemonType? teraType,
    PokemonSize size,
    AbilitySlot abilitySlot,
    PokemonNature nature,
    EggCycles? eggCycles,
    int experience,
    IndividualValues individualValues,
    EffortValues? effortValues,
    int? vitality,
    int? stamina,
    Friendship? friendship,
    UserId userId,
    SpecimenId specimenId) : base(specimenId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, species.Id, nameof(species));

    WorldMismatchException.ThrowIfMismatch(Id, variety.Id, nameof(variety));
    if (variety.SpeciesId != species.Id)
    {
      throw new ArgumentException("The variety should belong to the species.", nameof(variety));
    }

    WorldMismatchException.ThrowIfMismatch(Id, form.Id, nameof(form));
    if (form.VarietyId != variety.Id)
    {
      throw new ArgumentException("The form should belong to the variety.", nameof(form));
    }

    InvalidGenderException.ThrowIfNotValid(variety.GenderRatio, gender, nameof(Gender));

    if (teraType.HasValue && !Enum.IsDefined(teraType.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(teraType));
    }
    teraType ??= form.Types.Primary;

    InvalidAbilitySlotException.ThrowIfNotValid(form.Abilities, abilitySlot, nameof(AbilitySlot));

    if (eggCycles is not null && eggCycles.Value > species.EggCycles.Value)
    {
      eggCycles = species.EggCycles;
    }

    ArgumentOutOfRangeException.ThrowIfNegative(experience, nameof(experience));

    if (eggCycles is not null && experience > 0)
    {
      throw new ArgumentException("An egg Pokémon cannot have experience.", nameof(experience));
    }

    GrowthRate growthRate = species.GrowthRate;
    int level = ExperienceTable.Instance.GetLevel(growthRate, experience);

    effortValues ??= new();
    PokemonStatistics statistics = new(form.BaseStatistics, individualValues, effortValues, level, nature);

    if (vitality < MinimumVitality || vitality > MaximumVitality)
    {
      throw new ArgumentOutOfRangeException(nameof(vitality));
    }
    if (vitality is null || vitality > statistics.HP)
    {
      vitality = statistics.HP;
    }

    if (stamina < MinimumStamina || stamina > MaximumStamina)
    {
      throw new ArgumentOutOfRangeException(nameof(stamina));
    }
    if (stamina is null || stamina > statistics.HP)
    {
      stamina = statistics.HP;
    }

    PokemonCreated created = new(species.Id, variety.Id, form.Id, key ?? species.Key, gender, isShiny, teraType.Value, size, abilitySlot, nature, growthRate,
      eggCycles, experience, form.BaseStatistics, individualValues, effortValues, vitality.Value, stamina.Value, friendship ?? species.BaseFriendship);
    Raise(created, userId.ActorId);
  }
  protected virtual void Handle(PokemonCreated @event)
  {
    SpeciesId = @event.SpeciesId;
    VarietyId = @event.VarietyId;
    FormId = @event.FormId;

    _key = @event.Key;
    Gender = @event.Gender;
    IsShiny = @event.IsShiny;

    TeraType = @event.TeraType;
    _size = @event.Size;
    AbilitySlot = @event.AbilitySlot;
    _nature = @event.Nature;

    GrowthRate = @event.GrowthRate;
    EggCycles = @event.EggCycles;
    Experience = @event.Experience;

    _baseStatistics = @event.BaseStatistics;
    _individualValues = @event.IndividualValues;
    _effortValues = @event.EffortValues;
    Vitality = @event.Vitality;
    Stamina = @event.Stamina;
    _friendship = @event.Friendship;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new PokemonDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, SizeBytes);

  public void Nickname(Name? name, UserId userId)
  {
    if (Name != name)
    {
      Raise(new PokemonNicknamed(name), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonNicknamed @event)
  {
    Name = @event.Name;
  }

  public void RemoveHeldItem(UserId userId)
  {
    if (HeldItemId.HasValue)
    {
      Raise(new PokemonHeldItemChanged(ItemId: null), userId.ActorId);
    }
  }

  public void SetHeldItem(Item item, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, item.Id, nameof(item));

    if (HeldItemId != item.Id)
    {
      Raise(new PokemonHeldItemChanged(item.Id), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonHeldItemChanged @event)
  {
    HeldItemId = @event.ItemId;
  }

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new PokemonKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new PokemonUpdated();
    }
  }
  protected virtual void Handle(PokemonUpdated @event)
  {
    if (@event.Sprite is not null)
    {
      _sprite = @event.Sprite.Value;
    }
    if (@event.Url is not null)
    {
      _url = @event.Url.Value;
    }
    if (@event.Notes is not null)
    {
      _notes = @event.Notes.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
