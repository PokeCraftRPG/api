using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveMechanicsChanged(Accuracy? Accuracy, Power? Power, PowerPoints? PowerPoints) : DomainEvent;
