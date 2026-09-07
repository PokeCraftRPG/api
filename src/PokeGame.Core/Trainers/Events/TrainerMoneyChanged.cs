using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerMoneyChanged(Money Money) : DomainEvent;
