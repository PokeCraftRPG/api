using Krakenar.Contracts;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

public class UnexpectedVersionException : ErrorException
{
  private const string ErrorMessage = "The entity version did not match the expected version.";

  public string StreamId
  {
    get => (string)Data[nameof(StreamId)]!;
    private set => Data[nameof(StreamId)] = value;
  }
  public string EventId
  {
    get => (string)Data[nameof(EventId)]!;
    private set => Data[nameof(EventId)] = value;
  }
  public long ExpectedVersion
  {
    get => (long)Data[nameof(ExpectedVersion)]!;
    private set => Data[nameof(ExpectedVersion)] = value;
  }
  public long ActualVersion
  {
    get => (long)Data[nameof(ActualVersion)]!;
    private set => Data[nameof(ActualVersion)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(StreamId)] = StreamId;
      error.Data[nameof(EventId)] = EventId;
      error.Data[nameof(ExpectedVersion)] = ExpectedVersion;
      error.Data[nameof(ActualVersion)] = ActualVersion;
      return error;
    }
  }

  public UnexpectedVersionException(DomainEvent @event, long actualVersion)
    : base(BuildMessage(@event, actualVersion))
  {
    StreamId = @event.StreamId.Value;
    EventId = @event.Id.Value;
    ExpectedVersion = @event.Version - 1;
    ActualVersion = actualVersion;
  }

  internal static void ThrowIfUnexpected(DomainEvent @event, [NotNull] AggregateEntity? aggregate)
  {
    long expectedVersion = @event.Version - 1;
    long actualVersion = aggregate?.Version ?? 0;
    if (aggregate is null || actualVersion != expectedVersion)
    {
      throw new UnexpectedVersionException(@event, actualVersion);
    }
  }

  private static string BuildMessage(DomainEvent @event, long actualVersion) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(StreamId), @event.StreamId)
    .AddData(nameof(EventId), @event.Id)
    .AddData(nameof(ExpectedVersion), @event.Version)
    .AddData(nameof(ActualVersion), actualVersion)
    .Build();
}
