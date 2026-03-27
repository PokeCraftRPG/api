using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;

namespace PokeGame.Core;

public readonly struct UserId
{
  public ActorId ActorId { get; }
  public string Value => ActorId.Value;

  // TODO(fpion): RealmId
  // TODO(fpion): EntityId

  public UserId(ActorId actorId)
  {
    _ = Entity.Parse(actorId.Value, ActorType.User.ToString());
    ActorId = actorId;
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
