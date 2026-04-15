namespace PokeGame.Core.Items.Properties;

public abstract record ItemProperties
{
  [JsonIgnore]
  public abstract ItemCategory Category { get; }
}
