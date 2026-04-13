using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Inventory
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Inventory), alias: null);

  public static readonly ColumnId ItemId = new(nameof(InventoryEntity.ItemId), Table);
  public static readonly ColumnId Quantity = new(nameof(InventoryEntity.Quantity), Table);
  public static readonly ColumnId TrainerId = new(nameof(InventoryEntity.TrainerId), Table);
}
