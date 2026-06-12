using Logitar;
using Logitar.EventSourcing;
using PokeGame.Infrastructure.Outbox;

namespace PokeGame.Infrastructure.Entities;

internal class OutboxMessageEntity
{
  public long OutboxMessageId { get; private set; }

  public string StreamId { get; private set; } = string.Empty;
  public string EventId { get; private set; } = string.Empty;
  public long Version { get; private set; }

  public DateTime UpdatedOn { get; private set; }

  public OutboxMessageStatus Status { get; private set; }
  public string? Error { get; private set; }

  public OutboxMessageEntity(DomainEvent @event, DateTime? timestamp = null)
  {
    StreamId = @event.StreamId.Value;
    EventId = @event.Id.Value;
    Version = @event.Version;

    UpdatedOn = (timestamp ?? DateTime.Now).AsUniversalTime();
  }

  private OutboxMessageEntity()
  {
  }

  public void Fail(string error)
  {
    Status = OutboxMessageStatus.Failed;
    Error = error;
  }

  public override bool Equals(object? obj) => obj is OutboxMessageEntity message && message.OutboxMessageId == OutboxMessageId;
  public override int GetHashCode() => OutboxMessageId.GetHashCode();
  public override string ToString() => $"{base.ToString()} (OutboxMessageId={OutboxMessageId})";
}
