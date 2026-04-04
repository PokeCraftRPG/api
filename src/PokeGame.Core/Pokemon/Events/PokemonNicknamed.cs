using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonNicknamed(Name? Name) : DomainEvent;
