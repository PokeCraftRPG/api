using Logitar.EventSourcing;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonNicknameChanged(Name? Nickname) : DomainEvent;
