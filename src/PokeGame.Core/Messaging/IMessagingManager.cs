using Logitar.EventSourcing;

namespace PokeGame.Core.Messaging;

public interface IMessagingManager
{
  Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
  Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
}
