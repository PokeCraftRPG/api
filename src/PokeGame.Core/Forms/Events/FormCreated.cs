using Logitar.EventSourcing;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Events;

public sealed record FormCreated(
  SpeciesId SpeciesId,
  VarietyId VarietyId,
  FormCategory Category,
  Key Key,
  FormTypes Types,
  FormAbilities Abilities,
  BaseStatistics Statistics,
  FormYield Yield,
  FormSize Size) : DomainEvent;
