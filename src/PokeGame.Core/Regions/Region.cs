using Logitar.EventSourcing;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public class Region : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Region";

  public new RegionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The key was not initialized.");
  public Name? Name { get; private set; }
  public Description? Description { get; private set; }

  public Region() : base()
  {
  }

  public Region(World world, Slug key, ActorId? actorId = null)
    : this(RegionId.NewId(world.Id), key, actorId)
  {
  }

  public Region(RegionId regionId, Slug key, ActorId? actorId = null)
    : base(regionId.StreamId)
  {
    Raise(new RegionCreated(key), actorId);
  }
  protected virtual void Handle(RegionCreated @event)
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

  public void Describe(Description? description, ActorId? actorId = null)
  {
    if (!Equals(Description, description))
    {
      Raise(new RegionDescribed(description), actorId);
    }
  }
  protected virtual void Handle(RegionDescribed @event)
  {
    Description = @event.Description;
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void Rename(Name? name, ActorId? actorId = null)
  {
    if (!Equals(Name, name))
    {
      Raise(new RegionRenamed(name), actorId);
    }
  }
  protected virtual void Handle(RegionRenamed @event)
  {
    Name = @event.Name;
  }

  public void SetKey(Slug key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new RegionKeyChanged(key), actorId);
    }
  }
  protected virtual void Handle(RegionKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
