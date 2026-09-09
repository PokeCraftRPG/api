using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
