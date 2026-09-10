using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonStatusChanged(int Vitality, int Stamina, StatusCondition? Condition, Friendship Friendship) : DomainEvent;
