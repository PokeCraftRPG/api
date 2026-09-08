using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public sealed record TrainerLicenseChanged(License? License) : DomainEvent;
