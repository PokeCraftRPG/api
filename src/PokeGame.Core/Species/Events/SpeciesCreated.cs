using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesCreated(Number Number, PokemonCategory Category, Slug Key) : DomainEvent;
