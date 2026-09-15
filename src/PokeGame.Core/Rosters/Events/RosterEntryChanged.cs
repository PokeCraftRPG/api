using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterEntryChanged(PokemonId PokemonId, int Priority, IReadOnlySet<Guid> TagIds) : DomainEvent;
