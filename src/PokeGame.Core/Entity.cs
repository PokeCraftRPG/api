using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public interface IEntityProvider
{
  Entity GetEntity();
}

public sealed class Entity
{
  private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
  private const char EntitySeparator = ':';
  private const char ScopeSeparator = '|';

  public WorldId? WorldId { get; }
  public string Kind { get; }
  public Guid Id { get; }

  public Entity(string kind, Guid id, WorldId? worldId = null)
  {
    if (string.IsNullOrEmpty(kind))
    {
      throw new ArgumentException("The kind is required.", nameof(kind));
    }
    else if (kind.Any(c => !AllowedCharacters.Contains(c)))
    {
      throw new ArgumentOutOfRangeException(nameof(kind), "The kind must contain ASCII letters only.");
    }

    WorldId = worldId;
    Kind = kind;
    Id = id;
  }

  public static Entity Parse(string value, string? expectedKind = null)
  {
    string[] values = value.Split(ScopeSeparator);
    if (values.Length > 2)
    {
      throw new ArgumentException($"The value '{value}' is not a valid entity identifier.", nameof(value));
    }

    WorldId? worldId = values.Length == 2 ? new(values[0]) : null;
    string[] entity = values[^1].Split(EntitySeparator);
    if (entity.Length != 2)
    {
      throw new ArgumentException($"The value '{values[^1]}' is not a valid entity.", nameof(value));
    }

    string kind = entity[0];
    if (expectedKind is not null && expectedKind != kind)
    {
      throw new ArgumentException($"The entity kind '{kind}' was not expected ({expectedKind}).", nameof(value));
    }
    Guid id = new(Convert.FromBase64String(entity[1].FromUriSafeBase64()));

    return new Entity(kind, id, worldId);
  }

  public override bool Equals(object? obj) => obj is Entity entity && entity.WorldId == WorldId && entity.Kind == Kind && entity.Id == Id;
  public override int GetHashCode() => HashCode.Combine(WorldId, Kind, Id);
  public override string ToString()
  {
    string entity = string.Join(EntitySeparator, Kind, Convert.ToBase64String(Id.ToByteArray()).ToUriSafeBase64());
    return WorldId.HasValue ? string.Join(ScopeSeparator, WorldId.Value, entity) : entity;
  }
}
