using Logitar.EventSourcing;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Events;

public record FormCreated(
  VarietyId VarietyId,
  bool IsDefault,
  Slug Key,
  Height Height,
  Weight Weight,
  Types Types,
  BaseStatistics BaseStatistics,
  Yield Yield,
  Sprites Sprites) : DomainEvent;

/* TODO(fpion):
 * Abilities
 */
