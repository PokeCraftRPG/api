using Logitar.Data;

namespace PokeGame.Infrastructure.Db;

public static class History
{
  public static readonly TableId Table = new(nameof(PokemonContext.History));

  public static readonly ColumnId EventData = new(nameof(HistoryRecord.EventData), Table);
  public static readonly ColumnId EventId = new(nameof(HistoryRecord.EventId), Table);
  public static readonly ColumnId EventType = new(nameof(HistoryRecord.EventType), Table);
  public static readonly ColumnId HistoryRecordId = new(nameof(HistoryRecord.HistoryRecordId), Table);
  public static readonly ColumnId OccurredOn = new(nameof(HistoryRecord.OccurredOn), Table);
  public static readonly ColumnId ResourceId = new(nameof(HistoryRecord.ResourceId), Table);
  public static readonly ColumnId ResourceKind = new(nameof(HistoryRecord.ResourceKind), Table);
  public static readonly ColumnId UserId = new(nameof(HistoryRecord.UserId), Table);
  public static readonly ColumnId Version = new(nameof(HistoryRecord.Version), Table);
  public static readonly ColumnId WorldId = new(nameof(HistoryRecord.WorldId), Table);
}
