using Logitar.EventSourcing;
using PokeGame.Core.Forms.Events;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public sealed class Form : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Form";

  public new FormId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public VarietyId VarietyId { get; private set; }
  public FormCategory Category { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public FormTypes Types { get; private set; } = new();
  private FormAbilities? _abilities = null;
  public FormAbilities Abilities => _abilities ?? throw new InvalidOperationException("The abilities were not initialized.");
  private BaseStatistics? _baseStatistics = null;
  public BaseStatistics BaseStatistics => _baseStatistics ?? throw new InvalidOperationException("The base statistics were not initialized.");
  private FormYield? _yield = null;
  public FormYield Yield => _yield ?? throw new InvalidOperationException("The yield was not initialized.");
  private FormSize? _size = null;
  public FormSize Size => _size ?? throw new InvalidOperationException("The size was not initialized.");

  public FormSprites? Sprites { get; private set; }

  public Form() : base()
  {
  }

  public Form(
    Variety variety,
    FormCategory category,
    Key key,
    FormTypes types,
    FormAbilities abilities,
    BaseStatistics statistics,
    FormYield yield,
    FormSize size,
    ActorId? actorId = null) : this(FormId.NewId(variety.WorldId), category, variety.Id, key, types, abilities, statistics, yield, size, actorId)
  {
  }

  public Form(
    FormId formId,
    FormCategory category,
    VarietyId varietyId,
    Key key,
    FormTypes types,
    FormAbilities abilities,
    BaseStatistics statistics,
    FormYield yield,
    FormSize size,
    ActorId? actorId = null) : base(formId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, varietyId, nameof(varietyId));
    EnsureSameWorld(abilities, nameof(abilities));

    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new FormCreated(varietyId, category, key, types, abilities, statistics, yield, size), actorId);
  }
  private void Handle(FormCreated @event)
  {
    VarietyId = @event.VarietyId;
    Category = @event.Category;

    _key = @event.Key;

    Types = @event.Types;
    _abilities = @event.Abilities;
    _baseStatistics = @event.Statistics;
    _yield = @event.Yield;
    _size = @event.Size;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new FormDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new FormDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(FormDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetCharacteristics(FormTypes types, FormAbilities abilities, BaseStatistics baseStatistics, FormYield yield, FormSize size, ActorId? actorId = null)
  {
    EnsureSameWorld(abilities, nameof(abilities));

    if (!Equals(Types, types) || !Equals(Abilities, abilities) || !Equals(BaseStatistics, baseStatistics) || !Equals(Yield, yield) || !Equals(Size, size))
    {
      Raise(new FormCharacteristicsChanged(types, abilities, baseStatistics, yield, size), actorId);
    }
  }
  private void Handle(FormCharacteristicsChanged @event)
  {
    Types = @event.Types;
    _abilities = @event.Abilities;
    _baseStatistics = @event.BaseStatistics;
    _yield = @event.Yield;
    _size = @event.Size;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new FormKeyChanged(key), actorId);
    }
  }
  private void Handle(FormKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetSprites(FormSpriteAssets? assets, ActorId? actorId = null)
  {
    FormSprites? sprites = assets?.ToSprites(this);
    if (!Equals(Sprites, sprites))
    {
      Raise(new FormSpritesChanged(sprites), actorId);
    }
  }
  private void Handle(FormSpritesChanged @event)
  {
    Sprites = @event.Sprites;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";

  private void EnsureSameWorld(FormAbilities abilities, string paramName)
  {
    WorldMismatchException.ThrowIfMismatch(this, abilities.PrimaryId, paramName);
    if (abilities.SecondaryId.HasValue)
    {
      WorldMismatchException.ThrowIfMismatch(this, abilities.SecondaryId.Value, paramName);
    }
    if (abilities.HiddenId.HasValue)
    {
      WorldMismatchException.ThrowIfMismatch(this, abilities.HiddenId.Value, paramName);
    }
  }
}
