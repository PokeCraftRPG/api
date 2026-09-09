using Logitar.EventSourcing;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonDetailsChanged(Summary? Summary, Content? Content) : DomainEvent;
