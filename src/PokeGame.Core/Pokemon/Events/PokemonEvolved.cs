using Logitar.EventSourcing;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonEvolved(
  SpeciesId SpeciesId,
  VarietyId VarietyId,
  FormId FormId,
  BaseStatistics BaseStatistics,
  int Vitality,
  int Stamina) : DomainEvent;
