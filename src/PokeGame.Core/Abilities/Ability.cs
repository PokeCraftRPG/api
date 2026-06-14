using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public class Ability : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Ability";

  public new AbilityId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The key was not initialized.");
  public Name? Name { get; private set; }
  public Description? Description { get; private set; }

  public Ability() : base()
  {
  }

  public Ability(World world, Slug key, ActorId? actorId = null)
    : this(AbilityId.NewId(world.Id), key, actorId)
  {
  }

  public Ability(AbilityId abilityId, Slug key, ActorId? actorId = null)
    : base(abilityId.StreamId)
  {
    Raise(new AbilityCreated(key), actorId);
  }
  protected virtual void Handle(AbilityCreated @event)
  {
    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new AbilityDeleted(), actorId);
    }
  }

  public void Describe(Description? description, ActorId? actorId = null)
  {
    if (!Equals(Description, description))
    {
      Raise(new AbilityDescribed(description), actorId);
    }
  }
  protected virtual void Handle(AbilityDescribed @event)
  {
    Description = @event.Description;
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void Rename(Name? name, ActorId? actorId = null)
  {
    if (!Equals(Name, name))
    {
      Raise(new AbilityRenamed(name), actorId);
    }
  }
  protected virtual void Handle(AbilityRenamed @event)
  {
    Name = @event.Name;
  }

  public void SetKey(Slug key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new AbilityKeyChanged(key), actorId);
    }
  }
  protected virtual void Handle(AbilityKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
