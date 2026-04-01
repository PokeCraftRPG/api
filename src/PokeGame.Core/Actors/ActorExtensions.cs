using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;

namespace PokeGame.Core.Actors;

public static class ActorExtensions
{
  private const string RealmKind = "Realm";
  private const char Separator = '|';

  public static ActorId GetActorId(this Actor actor)
  {
    Entity? realm = actor.RealmId.HasValue ? new(RealmKind, actor.RealmId.Value) : null;
    Entity entity = new(actor.Type.ToString(), actor.Id);
    return new ActorId(realm is null ? entity.ToString() : string.Join(Separator, realm, entity));
  }

  public static UserId GetUserId(this User user) => new(new Actor(user).GetActorId());

  public static Actor ToActor(this ActorId actorId)
  {
    string[] values = actorId.Value.Split(Separator);
    if (values.Length < 1 || values.Length > 2)
    {
      throw new ArgumentException($"The value '{actorId}' is not a valid actor identifier.", nameof(actorId));
    }

    Entity? realm = values.Length == 2 ? Entity.Parse(values.First(), RealmKind) : null;
    Entity entity = Entity.Parse(values.Last());
    if (!Enum.TryParse(entity.Kind, ignoreCase: true, out ActorType type) || !Enum.IsDefined(type))
    {
      throw new ArgumentException($"The actor type '{entity.Kind}' is not valid.", nameof(actorId));
    }

    return new Actor
    {
      RealmId = realm?.Id,
      Id = entity.Id,
      Type = type
    };
  }
}
