using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using PokeGame.Core.Actors;

namespace PokeGame.Core;

public readonly struct UserId
{
  public ActorId ActorId { get; }
  public string Value => ActorId.Value;

  public Guid? RealmId { get; }
  public Guid EntityId { get; }

  public UserId(ActorId actorId)
  {
    Actor actor = actorId.ToActor();
    if (actor.Type != ActorType.User)
    {
      throw new ArgumentException("The actor must be a user.", nameof(actorId));
    }

    ActorId = actorId;
    RealmId = actor.RealmId;
    EntityId = actor.Id;
  }

  public UserId(string value) : this(new ActorId(value))
  {
  }

  public static bool operator ==(UserId left, UserId right) => left.Equals(right);
  public static bool operator !=(UserId left, UserId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is UserId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
