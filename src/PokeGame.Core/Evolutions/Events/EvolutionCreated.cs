using Logitar.EventSourcing;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;

namespace PokeGame.Core.Evolutions.Events;

public sealed record EvolutionCreated(FormId SourceId, FormId TargetId, EvolutionTrigger Trigger, ItemId? ItemId) : DomainEvent;
