using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Species;
using PokeGame.Core.Specimens.Events;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Specimens;

public sealed class Specimen : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Specimen";

  public new SpecimenId Id => new(base.Id);
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
  private readonly PokemonSize? _size = null;
  public PokemonSize Size => _size ?? throw new InvalidOperationException("The size was not initialized.");
  private readonly PokemonNature? _nature = null;
  public PokemonNature Nature => _nature ?? throw new InvalidOperationException("The nature was not initialized.");

  public int EggCycles { get; private set; }
  public bool IsEgg => EggCycles > 0;
  public GrowthRate GrowthRate { get; private set; }
  public int Experience { get; private set; }
  // TODO(fpion): Level

  private BaseStatistics? _baseStatistics = null;
  private IndividualValues? _individualValues = null;
  private EffortValues? _effortValues = null;
  // TODO(fpion): Statistics

  public int Vitality { get; private set; }
  public int Stamina { get; private set; }
  public bool IsFainted => Vitality < 1;
  public StatusCondition? Condition { get; private set; }
  public Friendship Friendship { get; private set; } = new();

  // TODO(fpion): Characteristic

  public ItemId? HeldItemId { get; private set; }

  // TODO(fpion): Moves

  // TODO(fpion): Ownership

  public AssetId? SpriteId { get; private set; }

  public Specimen() : base()
  {
  }

  public Specimen(
    SpecimenId specimenId,
    PokemonSpecies species,
    Variety variety,
    Form form,
    // TODO(fpion): size
    // TODO(fpion): nature
    Key? key = null,
    Gender? gender = null,
    bool? isShiny = null,
    PokemonType? teraType = null,
    AbilitySlot? abilitySlot = null,
    // TODO(fpion): egg cycles
    // TODO(fpion): experience
    IndividualValues? individualValues = null,
    EffortValues? effortValues = null,
    int? vitality = null,
    int? stamina = null,
    Friendship? friendship = null,
    ActorId? actorId = null)
    : base(specimenId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, species, nameof(species));

    WorldMismatchException.ThrowIfMismatch(this, variety, nameof(variety));
    if (variety.SpeciesId != species.Id)
    {
      throw new NotImplementedException(); // TODO(fpion): ArgumentException
    }

    WorldMismatchException.ThrowIfMismatch(this, form, nameof(form));
    if (form.VarietyId != variety.Id)
    {
      throw new NotImplementedException(); // TODO(fpion): ArgumentException
    }
    // TODO(fpion): should we validate against form.Category?

    if (gender.HasValue && !Enum.IsDefined(gender.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(gender));
    }
    // TODO(fpion): validate gender against variety.GenderRatio
    // TODO(fpion): randomize gender

    if (teraType.HasValue && !Enum.IsDefined(teraType.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(teraType));
    }

    if (abilitySlot.HasValue && !Enum.IsDefined(abilitySlot.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(abilitySlot));
    }
    // TODO(fpion): validate ability slot against form.Abilities
    abilitySlot ??= AbilitySlot.Primary; // TODO(fpion): randomize

    key ??= species.Key;
    isShiny ??= null!; // TODO(fpion): randomize
    teraType ??= null!; // TODO(fpion): randomize
    individualValues ??= null!; // TODO(fpion): randomize
    effortValues ??= new();
    int experience = 0;
    vitality ??= 0;
    stamina ??= 0;
    friendship ??= species.BaseFriendship;

    PokemonCreated @event = new(
      species.Id,
      variety.Id,
      form.Id,
      key,
      gender,
      isShiny.Value,
      teraType.Value,
      abilitySlot.Value,
      species.GrowthRate,
      experience,
      form.BaseStatistics,
      individualValues,
      effortValues,
      vitality.Value,
      stamina.Value,
      friendship);
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

    _baseStatistics = @event.BaseStatistics;
    _individualValues = @event.IndividualValues;
    _effortValues = @event.EffortValues;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new PokemonDeleted(), actorId);
    }
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

  public override string ToString() => $"{Nickname?.Value ?? Key.Value} | {base.ToString()}";
}
