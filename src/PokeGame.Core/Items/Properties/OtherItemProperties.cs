namespace PokeGame.Core.Items.Properties;

public interface IOtherItemProperties;

public record OtherItemProperties : ItemProperties, IOtherItemProperties
{
  [JsonIgnore]
  public override ItemCategory Category { get; } = ItemCategory.OtherItem;

  [JsonConstructor]
  public OtherItemProperties()
  {
  }

  public OtherItemProperties(IOtherItemProperties _) : this()
  {
  }
}
