using Logitar.EventSourcing;

namespace PokeGame.Core.Messaging;

public interface IMessagingManager
{
  Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
}
