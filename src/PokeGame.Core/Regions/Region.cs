using Logitar.EventSourcing;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public sealed class Region : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Region";

  public new RegionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Region() : base()
  {
  }

  public Region(World world, Key key, ActorId? actorId = null)
    : this(RegionId.NewId(world.Id), key, actorId)
  {
  }

  public Region(RegionId regionId, Key key, ActorId? actorId = null)
    : base(regionId.StreamId)
  {
    Raise(new RegionCreated(key), actorId);
  }
  private void Handle(RegionCreated @event)
  {
    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new RegionDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new RegionKeyChanged(key), actorId);
    }
  }
  private void Handle(RegionKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new RegionUpdated(name, summary, content), actorId);
    }
  }
  private void Handle(RegionUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
