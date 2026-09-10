using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonDetailsChanged(Summary? Summary, Content? Content) : DomainEvent;
