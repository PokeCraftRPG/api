using Logitar.EventSourcing;

namespace PokeGame;

public static class AggregateExtensions
{
  public static TEvent LastChange<TEvent>(this AggregateRoot aggregate) where TEvent : class, IEvent
  {
    Assert.NotEmpty(aggregate.Changes);
    return Assert.IsType<TEvent>(aggregate.Changes.Last());
  }

  public static T Replay<T>(this T source) where T : AggregateRoot, new()
  {
    T destination = new();
    AggregateRoot aggregate = source;
    destination.LoadFromChanges(aggregate.Id, aggregate.Changes);
    return destination;
  }
}
