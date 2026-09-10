using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Inventory.Models;

public record InventoryItemDto
{
  public ItemDto Item { get; set; } = new();
  public int Quantity { get; set; }
}
