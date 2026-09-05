using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public sealed class Ability : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Ability";

  public new AbilityId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Ability() : base()
  {
  }

  public Ability(World world, Key key, ActorId? actorId = null)
    : this(AbilityId.NewId(world.Id), key, actorId)
  {
  }

  public Ability(AbilityId abilityId, Key key, ActorId? actorId = null)
    : base(abilityId.StreamId)
  {
    Raise(new AbilityCreated(key), actorId);
  }
  private void Handle(AbilityCreated @event)
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

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new AbilityDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(AbilityDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new AbilityKeyChanged(key), actorId);
    }
  }
  private void Handle(AbilityKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
