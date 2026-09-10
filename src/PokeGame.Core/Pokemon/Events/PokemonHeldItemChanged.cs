using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonHeldItemChanged(ItemId? HeldItemId) : DomainEvent;
