using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerMemberChanged(UserId? MemberId) : DomainEvent;
