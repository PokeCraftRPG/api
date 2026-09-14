using Logitar.EventSourcing;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

public class UnexpectedVersionException : Exception
{
  public UnexpectedVersionException(DomainEvent @event, long actualVersion)
    : base("The entity version did not match the expected version.")
  {
    Data["StreamId"] = @event.StreamId.Value;
    Data["EventId"] = @event.Id.Value;
    Data["ExpectedVersion"] = @event.Version - 1;
    Data["ActualVersion"] = actualVersion;
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
}
