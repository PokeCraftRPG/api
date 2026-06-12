using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public interface IEntityProvider
{
  Entity GetEntity();
}

public record Entity
{
  private const char Separator = '|';
  private const char EntitySeparator = ':';

  public string Kind { get; }
  public Guid Id { get; }
  public WorldId? WorldId { get; }

  public Entity(string kind, Guid id, WorldId? worldId = null)
  {
    if (string.IsNullOrWhiteSpace(kind))
    {
      throw new ArgumentException("The entity kind is required.", nameof(kind));
    }

    Kind = kind.Trim();
    Id = id;
    WorldId = worldId;
  }

  public static Entity Parse(string value, string? expectedKind = null)
  {
    string[] values = value.Split(Separator);
    if (values.Length > 2)
    {
      throw new ArgumentException($"The value '{value}' is not a valid entity identifier.", nameof(value));
    }

    WorldId? worldId = values.Length == 2 ? new(values.First()) : null;

    string[] entity = values.Last().Split(EntitySeparator);
    if (entity.Length != 2)
    {
      throw new ArgumentException($"The value '{values.Last()}' is not a valid entity.", nameof(value));
    }

    string entityKind = entity.First();
    if (expectedKind is not null && expectedKind != entityKind)
    {
      throw new ArgumentException($"The entity kind '{entityKind}' was not the expected '{expectedKind}'.", nameof(value));
    }
    Guid entityId = new(Convert.FromBase64String(entity.Last().FromUriSafeBase64()));

    return new Entity(entityKind, entityId, worldId);
  }

  public override string ToString()
  {
    string entity = string.Join(EntitySeparator, Kind, Convert.ToBase64String(Id.ToByteArray()).ToUriSafeBase64());
    return WorldId.HasValue ? string.Join(Separator, WorldId.Value, entity) : entity;
  }
}
