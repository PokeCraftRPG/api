using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
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

  public new PokemonId Id => new(base.Id);
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
  public bool IsFainted => Vitality < 1;
  public int Stamina { get; private set; }
  public bool IsUnconscious => IsFainted || Stamina < 1;
  public StatusCondition? StatusCondition { get; private set; }
  private Friendship? _friendship = null;
  public Friendship Friendship => _friendship ?? throw new InvalidOperationException("The Pokémon has not been initialized.");

  public PokemonCharacteristic Characteristic => PokemonCharacteristics.Instance.Find(IndividualValues, Size);

  public ItemId? HeldItemId { get; private set; }

  public TrainerId? OriginalTrainerId { get; private set; }
  public PokemonOwnership? Ownership { get; private set; }
  public PokemonSlot? Slot { get; private set; }

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
    PokemonSpecies species,
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
    Friendship? friendship) : this(species, variety, form, key, gender, isShiny, teraType, size, abilitySlot, nature, eggCycles, experience, individualValues, effortValues, vitality, stamina, friendship, world.OwnerId, PokemonId.NewId(world.Id))
  {
  }

  public Specimen(
    PokemonSpecies species,
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
    PokemonId pokemonId) : base(pokemonId.StreamId)
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

    if (!Enum.IsDefined(abilitySlot))
    {
      throw new ArgumentOutOfRangeException(nameof(abilitySlot));
    }

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

  public void Catch(Trainer trainer, Item pokeBall, Location location, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, trainer.Id, nameof(trainer));
    WorldMismatchException.ThrowIfMismatch(Id, pokeBall.Id, nameof(pokeBall));

    if (pokeBall.Category != ItemCategory.PokeBall)
    {
      throw new InvalidItemException(pokeBall, ItemCategory.PokeBall, nameof(PokemonOwnership.PokeBallId));
    }

    if (Ownership is not null)
    {
      throw new CannotCatchOwnedPokemonException(this);
    }

    Raise(new PokemonCaught(trainer.Id, pokeBall.Id, new Level(Level), location, DateTime.Now), userId.ActorId);
  }
  protected virtual void Handle(PokemonCaught @event)
  {
    if (!IsEgg)
    {
      OriginalTrainerId = @event.TrainerId;
    }
    Ownership = new PokemonOwnership(OwnershipKind.Caught, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.MetOn);
  }

  public void ChangeForm(Form form, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, form.Id, nameof(form));

    if (form.VarietyId != VarietyId)
    {
      throw new InvalidFormException(this, form);
    }

    if (FormId != form.Id)
    {
      Raise(new PokemonFormChanged(form.Id, form.BaseStatistics), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonFormChanged @event)
  {
    FormId = @event.FormId;
    _baseStatistics = @event.BaseStatistics;

    PokemonStatistics statistics = new(BaseStatistics, IndividualValues, EffortValues, Level, Nature);
    Vitality = Math.Min(Vitality, statistics.HP);
    Stamina = Math.Min(Stamina, statistics.HP);
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new PokemonDeleted(), userId.ActorId);
    }
  }

  public void Deposit(PokemonSlot slot, UserId userId)
  {
    if (Ownership is null)
    {
      throw new InvalidOperationException($"The Pokémon 'Id={Id}' is not owned by any trainer.");
    }
    else if (Slot is null || Slot.Box.HasValue)
    {
      throw new InvalidOperationException($"The Pokémon 'Id={Id}' is not in the party of its owning trainer.");
    }
    else if (!slot.Box.HasValue)
    {
      throw new ArgumentException("The slot must have a box number.", nameof(slot));
    }

    Raise(new PokemonDeposited(slot.Position, slot.Box.Value), userId.ActorId);
  }
  protected virtual void Handle(PokemonDeposited @event)
  {
    Slot = new PokemonSlot(@event.Position, @event.Box);
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, SizeBytes);

  public void Gift(Trainer trainer, Location location, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, trainer.Id, nameof(trainer));

    if (Ownership is null)
    {
      throw new PokemonHasNoOwnerException(this);
    }
    else if (Ownership.TrainerId != trainer.Id)
    {
      Raise(new PokemonGifted(trainer.Id, Ownership.PokeBallId, new Level(Level), location, DateTime.Now), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonGifted @event)
  {
    Ownership = new PokemonOwnership(OwnershipKind.Gifted, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.MetOn);
  }

  public void Move(PokemonSlot slot, UserId userId)
  {
    if (Ownership is null)
    {
      throw new InvalidOperationException($"The Pokémon 'Id={Id}' is not owned by any trainer.");
    }
    else if (Slot != slot)
    {
      Raise(new PokemonMoved(slot), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonMoved @event)
  {
    Slot = @event.Slot;
  }

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

  public void Receive(Trainer trainer, Item pokeBall, Location location, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, trainer.Id, nameof(trainer));
    WorldMismatchException.ThrowIfMismatch(Id, pokeBall.Id, nameof(pokeBall));

    if (pokeBall.Category != ItemCategory.PokeBall)
    {
      throw new InvalidItemException(pokeBall, ItemCategory.PokeBall, nameof(PokemonOwnership.PokeBallId));
    }

    // TODO(fpion): do nothing when the trainer is the same?
    // TODO(fpion): we currently can change the Poké Ball and Location by calling Receive with the current trainer!

    Raise(new PokemonReceived(trainer.Id, pokeBall.Id, new Level(Level), location, DateTime.Now), userId.ActorId);
  }
  protected virtual void Handle(PokemonReceived @event)
  {
    OriginalTrainerId ??= @event.TrainerId; // TODO(fpion): remove the coalescing operator.
    Ownership = new PokemonOwnership(OwnershipKind.Received, @event.TrainerId, @event.PokeBallId, @event.Level, @event.Location, @event.MetOn);
  }

  public void RemoveHeldItem(UserId userId)
  {
    if (HeldItemId.HasValue)
    {
      Raise(new PokemonHeldItemChanged(ItemId: null), userId.ActorId);
    }
  }

  public void Release(UserId userId)
  {
    if (Ownership is not null)
    {
      if (IsEgg)
      {
        throw new CannotReleaseEggPokemonException(this);
      }

      Raise(new PokemonReleased(), userId.ActorId);
    }
  }
  protected virtual void Handle(PokemonReleased _)
  {
    Ownership = null;
    Slot = null;
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

  public void Withdraw(PokemonSlot slot, UserId userId)
  {
    if (Ownership is null)
    {
      throw new InvalidOperationException($"The Pokémon 'Id={Id}' is not owned by any trainer.");
    }
    else if (Slot is null || !Slot.Box.HasValue)
    {
      throw new InvalidOperationException($"The Pokémon 'Id={Id}' is already in the party of its owning trainer.");
    }
    else if (slot.Box.HasValue)
    {
      throw new ArgumentException("The slot must not have a box number.", nameof(slot));
    }

    Raise(new PokemonWithdrawn(slot.Position), userId.ActorId);
  }
  protected virtual void Handle(PokemonWithdrawn @event)
  {
    Slot = new PokemonSlot(@event.Position);
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
