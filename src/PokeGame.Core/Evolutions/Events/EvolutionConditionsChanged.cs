using Logitar.EventSourcing;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Evolutions.Events;

public sealed record EvolutionConditionsChanged(
  Level? Level,
  bool Friendship,
  Gender? Gender,
  ItemId? ItemId,
  MoveId? MoveId,
  Location? Location,
  TimeOfDay? TimeOfDay) : DomainEvent;
