using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonKeyChanged(Slug Key) : DomainEvent;
