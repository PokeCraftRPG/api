using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Core.Actors;

namespace PokeGame.Core.Identity;

public readonly struct UserId
{
  public ActorId ActorId { get; }
  public string Value => ActorId.Value;

  public Guid? RealmId { get; }
  public Guid EntityId { get; }

  public UserId(ActorId actorId)
  {
    ActorId = actorId;

    Actor actor = actorId.ToActor();
    if (actor.Type != ActorType.User)
    {
      throw new ArgumentException($"The actor '{actorId}' should be a user.", nameof(actorId));
    }
    RealmId = actor.RealmId;
    EntityId = actor.Id;
  }

  public UserId(string value) : this(new ActorId(value))
  {
  }

  public UserId(Guid entityId, Guid? realmId = null)
  {
    Actor actor = new()
    {
      RealmId = realmId,
      Id = entityId,
      Type = ActorType.User
    };
    ActorId = actor.ToActorId();

    RealmId = realmId;
    EntityId = entityId;
  }

  public UserId(User user) : this(user.Id, user.Realm?.Id)
  {
  }

  public static bool operator ==(UserId left, UserId right) => left.Equals(right);
  public static bool operator !=(UserId left, UserId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is UserId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
