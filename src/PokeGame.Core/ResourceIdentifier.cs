using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

public class ResourceIdentifier
{
  private const char Separator = '|';
  private const char ResourceSeparator = ':';

  private readonly string _value;

  public Guid? WorldId { get; }
  public string Kind { get; }
  public Guid Id { get; }

  public ResourceIdentifier(string kind, Guid id, Guid? worldId = null)
  {
    WorldId = worldId;
    Kind = kind.Trim();
    Id = id;

    string value = string.Join(ResourceSeparator, kind, Convert.ToBase64String(id.ToByteArray()).ToUriSafeBase64());
    _value = worldId.HasValue ? string.Join(Separator, new ResourceIdentifier(World.ResourceKind, worldId.Value), value) : value;
  }

  public static ResourceIdentifier Parse(string value, string? expectedKind = null)
  {
    string[] values = value.Split(Separator);
    if (values.Length > 2)
    {
      throw new ArgumentException($"The value '{value}' is not a valid resource identifier.", nameof(value));
    }

    Guid? worldId = values.Length == 2 ? ResourceIdentifier.Parse(values.First(), World.ResourceKind).Id : null;

    string[] parts = values.Last().Split(ResourceSeparator);
    if (parts.Length != 2)
    {
      throw new ArgumentException($"The value '{parts.Last()}' is not a valid resource.", nameof(value));
    }

    string kind = parts.First();
    if (expectedKind is not null && expectedKind != kind)
    {
      throw new ArgumentException($"The resource kind '{kind}' was not expected ({expectedKind}).");
    }
    Guid id = new(Convert.FromBase64String(parts.Last().FromUriSafeBase64()));

    return new ResourceIdentifier(kind, id, worldId);
  }

  public override bool Equals(object? obj) => obj is ResourceIdentifier identifier && identifier._value == _value;
  public override int GetHashCode() => _value.GetHashCode();
  public override string ToString() => _value;
}
