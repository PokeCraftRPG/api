using Logitar.EventSourcing;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Events;

public sealed record FormCreated(VarietyId VarietyId, FormCategory Category, Key Key) : DomainEvent;
