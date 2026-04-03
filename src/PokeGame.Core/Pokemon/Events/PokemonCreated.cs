using Logitar.EventSourcing;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonCreated(SpeciesId SpeciesId, VarietyId VarietyId, FormId FormId, Slug? Key, PokemonGender? Gender) : DomainEvent;
