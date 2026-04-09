using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class StorageSummary
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.StorageSummary), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(StorageSummaryEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(StorageSummaryEntity.CreatedOn), Table);
  public static readonly ColumnId StreamId = new(nameof(StorageSummaryEntity.StreamId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(StorageSummaryEntity.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(StorageSummaryEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(StorageSummaryEntity.Version), Table);

  public static readonly ColumnId AllocatedBytes = new(nameof(StorageSummaryEntity.AllocatedBytes), Table);
  public static readonly ColumnId RemainingBytes = new(nameof(StorageSummaryEntity.RemainingBytes), Table);
  public static readonly ColumnId UsedBytes = new(nameof(StorageSummaryEntity.UsedBytes), Table);
  public static readonly ColumnId WorldId = new(nameof(StorageSummaryEntity.WorldId), Table);
}
