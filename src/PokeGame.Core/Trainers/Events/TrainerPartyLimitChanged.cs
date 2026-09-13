using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerPartyLimitChanged(int? PartyLimit) : DomainEvent;
