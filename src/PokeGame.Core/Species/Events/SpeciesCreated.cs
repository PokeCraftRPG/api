using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesCreated(PokemonCategory Category, Slug Key) : DomainEvent;
