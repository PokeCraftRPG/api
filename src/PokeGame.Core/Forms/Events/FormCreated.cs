using Logitar.EventSourcing;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Events;

public record FormCreated(VarietyId VarietyId, bool IsDefault, Slug Key) : DomainEvent;

/* TODO(fpion):
 * IsBattleOnly & IsMega
 * Height & Weight
 * Types & Abilities
 * Base Statistics & Yield
 * Sprites
 */
