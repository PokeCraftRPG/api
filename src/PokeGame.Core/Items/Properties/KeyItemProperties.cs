namespace PokeGame.Core.Items.Properties;

public interface IKeyItemProperties;

public record KeyItemProperties : ItemProperties, IKeyItemProperties
{
  [JsonIgnore]
  public override ItemCategory Category { get; } = ItemCategory.KeyItem;

  [JsonConstructor]
  public KeyItemProperties()
  {
  }

  public KeyItemProperties(IKeyItemProperties _) : this()
  {
  }
}
