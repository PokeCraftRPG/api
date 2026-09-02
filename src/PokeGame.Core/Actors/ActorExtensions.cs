using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;

namespace PokeGame.Core.Actors;

public static class ActorExtensions
{
  public const string RealmKind = "Realm";
  public const char Separator = '|';

  public static Actor ToActor(this ActorId id)
  {
    string[] parts = id.Value.Split(Separator);
    if (parts.Length > 2)
    {
      throw new ArgumentException($"The value '{id}' is not a valid actor identifier.", nameof(id));
    }

    Entity? realm = parts.Length == 2 ? Entity.Parse(parts[0]) : null;
    Entity entity = Entity.Parse(parts[^1]);
    if (!Enum.TryParse(entity.Kind, out ActorType type) || !Enum.IsDefined(type))
    {
      throw new ArgumentOutOfRangeException(nameof(id), $"The actor type '{entity.Kind}' is not valid.");
    }

    return new Actor
    {
      RealmId = realm?.Id,
      Type = type,
      Id = entity.Id
    };
  }

  public static ActorId ToActorId(this Actor actor)
  {
    Entity? realm = actor.RealmId.HasValue ? new(RealmKind, actor.RealmId.Value) : null;
    Entity entity = new(actor.Type.ToString(), actor.Id);
    return new ActorId(realm is null ? entity.ToString() : string.Join(Separator, realm, entity));
  }
}
