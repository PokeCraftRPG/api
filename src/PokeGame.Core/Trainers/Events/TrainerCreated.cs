using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public record TrainerCreated(Slug Key, TrainerGender Gender) : DomainEvent; // TODO(fpion): License
