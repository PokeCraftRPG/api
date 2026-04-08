using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonCreated(
  SpeciesId SpeciesId,
  VarietyId VarietyId,
  FormId FormId,
  Slug Key,
  PokemonGender? Gender,
  bool IsShiny,
  PokemonType TeraType,
  PokemonSize Size,
  AbilitySlot AbilitySlot,
  PokemonNature Nature,
  GrowthRate GrowthRate,
  EggCycles? EggCycles,
  int Experience,
  BaseStatistics BaseStatistics,
  IndividualValues IndividualValues,
  EffortValues EffortValues,
  int Vitality,
  int Stamina,
  Friendship Friendship) : DomainEvent;
