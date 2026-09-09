using Logitar.EventSourcing;
using MassTransit;
using PokeGame.Core.Messaging;

namespace PokeGame.Infrastructure.Messaging;

internal class MessagingManager : IMessagingManager
{
  private readonly IPublishEndpoint _publishEndpoint;

  public MessagingManager(IPublishEndpoint publishEndpoint)
  {
    _publishEndpoint = publishEndpoint;
  }

  public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
  {
    await _publishEndpoint.Publish(@event, @event.GetType(), cancellationToken);
  }
}
