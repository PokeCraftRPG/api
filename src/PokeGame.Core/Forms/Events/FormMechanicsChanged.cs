using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormMechanicsChanged(FormTypes Types, FormAbilities Abilities, BaseStatistics BaseStatistics, FormYield Yield) : DomainEvent;
