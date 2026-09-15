using Logitar.EventSourcing;
using MassTransit;

namespace PokeGame.Infrastructure.Messaging;

internal static class MessagingExtensions
{
  private const string ActorIdKey = "ActorId";

  public static void SetActorId(this PublishContext context, ActorId? actorId)
  {
    if (actorId.HasValue)
    {
      context.Headers.Set(ActorIdKey, actorId.Value.Value);
    }
  }

  public static ActorId? GetActorId(this ConsumeContext context)
  {
    return context.Headers.TryGetHeader(ActorIdKey, out object? value) ? new ActorId((string)value) : null;
  }
}
