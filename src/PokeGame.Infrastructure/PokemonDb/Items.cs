using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Items
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Items), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(ItemEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(ItemEntity.CreatedOn), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(ItemEntity.UpdatedBy), Table);
  public static readonly ColumnId StreamId = new(nameof(ItemEntity.StreamId), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(ItemEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(ItemEntity.Version), Table);

  public static readonly ColumnId Category = new(nameof(ItemEntity.Category), Table);
  public static readonly ColumnId Description = new(nameof(ItemEntity.Description), Table);
  public static readonly ColumnId Id = new(nameof(ItemEntity.Id), Table);
  public static readonly ColumnId ItemId = new(nameof(ItemEntity.ItemId), Table);
  public static readonly ColumnId Key = new(nameof(ItemEntity.Key), Table);
  public static readonly ColumnId MoveId = new(nameof(ItemEntity.MoveId), Table);
  public static readonly ColumnId Name = new(nameof(ItemEntity.Name), Table);
  public static readonly ColumnId Notes = new(nameof(ItemEntity.Notes), Table);
  public static readonly ColumnId Price = new(nameof(ItemEntity.Price), Table);
  public static readonly ColumnId Properties = new(nameof(ItemEntity.Properties), Table);
  public static readonly ColumnId Sprite = new(nameof(ItemEntity.Sprite), Table);
  public static readonly ColumnId Url = new(nameof(ItemEntity.Url), Table);
  public static readonly ColumnId WorldId = new(nameof(ItemEntity.WorldId), Table);
}
