using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonCreated(
  SpeciesId SpeciesId,
  VarietyId VarietyId,
  FormId FormId,
  Key Key,
  Gender? Gender,
  bool IsShiny,
  PokemonType TeraType,
  AbilitySlot AbilitySlot,
  PokemonSize Size,
  PokemonNature Nature,
  byte EggCycles,
  GrowthRate GrowthRate,
  int Experience,
  BaseStatistics BaseStatistics,
  IndividualValues IndividualValues,
  int Vitality,
  int Stamina,
  Friendship Friendship,
  PokemonCharacteristic Characteristic) : DomainEvent;
