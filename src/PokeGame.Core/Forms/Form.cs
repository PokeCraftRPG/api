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

  public FormTypes? Types { get; private set; }
  public FormAbilities? Abilities { get; private set; }
  public FormSize? Size { get; private set; }
  public BaseStatistics? BaseStatistics { get; private set; }
  public Yield? Yield { get; private set; }
  public FormSprites? Sprites { get; private set; }

  public Form() : base()
  {
  }

  public Form(Variety variety, FormCategory category, Key key, ActorId? actorId = null)
    : this(FormId.NewId(variety.WorldId), category, variety.Id, key, actorId)
  {
  }

  public Form(FormId formId, FormCategory category, VarietyId varietyId, Key key, ActorId? actorId = null)
    : base(formId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(this, varietyId, nameof(varietyId));

    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new FormCreated(varietyId, category, key), actorId);
  }
  private void Handle(FormCreated @event)
  {
    VarietyId = @event.VarietyId;
    Category = @event.Category;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new FormDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

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

  public void Update(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new FormUpdated(name, summary, content), actorId);
    }
  }
  private void Handle(FormUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
