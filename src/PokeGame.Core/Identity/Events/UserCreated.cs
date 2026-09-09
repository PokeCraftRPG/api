using Logitar.EventSourcing;

namespace PokeGame.Core.Identity.Events;

public sealed record UserCreated(UserId UserId, EmailAddress EmailAddress) : IEvent;
