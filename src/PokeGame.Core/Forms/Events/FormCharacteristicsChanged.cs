using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormCharacteristicsChanged(
  FormTypes Types,
  FormAbilities Abilities,
  BaseStatistics BaseStatistics,
  FormYield Yield,
  FormSize Size) : DomainEvent;
