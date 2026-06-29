using Logitar;
using PokeGame.Core;

namespace PokeGame.Infrastructure;

internal class HistoryRecord
{
  public int HistoryRecordId { get; private set; }
  public Guid EventId { get; private set; }

  public Guid? WorldId { get; private set; }
  public string ResourceKind { get; private set; } = string.Empty;
  public Guid ResourceId { get; private set; }

  public long Version { get; private set; }
  public DateTime OccurredOn { get; private set; }
  public Guid? UserId { get; private set; }

  public string EventType { get; private set; } = string.Empty;
  public string EventData { get; private set; } = string.Empty;

  public HistoryRecord(ChangeEvent @event)
  {
    EventId = @event.EventId;

    WorldId = @event.WorldId;
    ResourceKind = @event.ResourceKind;
    ResourceId = @event.ResourceId;

    Version = @event.Version;
    OccurredOn = @event.OccurredOn.AsUniversalTime();
    UserId = @event.UserId;

    EventType = @event.GetType().GetNamespaceQualifiedName();
    EventData = EventSerializer.Instance.Serialize(@event);
  }

  private HistoryRecord()
  {
  }
}
