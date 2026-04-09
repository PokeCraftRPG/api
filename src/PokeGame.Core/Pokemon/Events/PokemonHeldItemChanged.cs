using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonHeldItemChanged(ItemId? ItemId) : DomainEvent;
