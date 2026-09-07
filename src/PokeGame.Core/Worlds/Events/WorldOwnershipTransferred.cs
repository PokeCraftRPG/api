using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldOwnershipTransferred(UserId UserId) : DomainEvent;
