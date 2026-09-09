using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Species;
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
  private readonly PokemonSize? _size = null;
  public PokemonSize Size => _size ?? throw new InvalidOperationException("The size was not initialized.");
  private readonly PokemonNature? _nature = null;
  public PokemonNature Nature => _nature ?? throw new InvalidOperationException("The nature was not initialized.");

  public byte EggCycles { get; private set; }
  public bool IsEgg => EggCycles > 0;
  public GrowthRate GrowthRate { get; private set; }
  public int Experience { get; private set; }
  public byte Level { get; private set; }

  private readonly BaseStatistics? _baseStatistics = null;
  private readonly IndividualValues? _individualValues = null;
  private readonly EffortValues? _effortValues = null;
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
    PokemonId specimenId,
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
    // TODO(fpion): raise PokemonCreated
  }
  private void Handle(PokemonCreated @event)
  {
    // TODO(fpion): handle PokemonCreated
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
