using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class StorageDetail
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.StorageDetail), alias: null);

  public static readonly ColumnId EntityId = new(nameof(StorageDetailEntity.EntityId), Table);
  public static readonly ColumnId EntityKind = new(nameof(StorageDetailEntity.EntityKind), Table);
  public static readonly ColumnId Key = new(nameof(StorageDetailEntity.Key), Table);
  public static readonly ColumnId Size = new(nameof(StorageDetailEntity.Size), Table);
  public static readonly ColumnId StorageDetailId = new(nameof(StorageDetailEntity.StorageDetailId), Table);
  public static readonly ColumnId WorldId = new(nameof(StorageDetailEntity.WorldId), Table);
}
