using PokeGame.Core.Items.Properties;

namespace PokeGame.Core.Items;

internal record UndefinedCategoryProperties : ItemProperties
{
  public override ItemCategory Category { get; } = (ItemCategory)(-1);
}
