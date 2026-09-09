using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonCreated(
  SpeciesId SpeciesId,
  VarietyId VarietyId,
  FormId FormId,
  Key Key,
  Gender? Gender,
  bool IsShiny,
  PokemonType TeraType,
  AbilitySlot AbilitySlot,
  // TODO(fpion): Size
  // TODO(fpion): Nature
  GrowthRate GrowthRate,
  // TODO(fpion): EggCycles
  int Experience,
  BaseStatistics BaseStatistics,
  IndividualValues IndividualValues,
  EffortValues EffortValues,
  int Vitality,
  int Stamina,
  Friendship Friendship) : DomainEvent;
