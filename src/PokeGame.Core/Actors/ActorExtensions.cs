using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;

namespace PokeGame.Core.Actors;

public static class ActorExtensions
{
  private const string RealmKind = "Realm";
  private const char Separator = '|';

  public static Actor ToActor(this ActorId id)
  {
    string[] values = id.Value.Split(Separator);
    if (values.Length > 2)
    {
      throw new ArgumentException($"The value '{id}' is not a valid actor identifier.", nameof(id));
    }

    Entity? realm = values.Length == 2 ? Entity.Parse(values.First(), RealmKind) : null;

    Entity entity = Entity.Parse(values.Last());
    if (!Enum.TryParse(entity.Kind, out ActorType type) || !Enum.IsDefined(type))
    {
      throw new ArgumentException($"The actor type '{entity.Kind}' is not valid.", nameof(id));
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

    string value = realm is null ? entity.ToString() : string.Join(Separator, realm, entity);
    return new ActorId(value);
  }
}
