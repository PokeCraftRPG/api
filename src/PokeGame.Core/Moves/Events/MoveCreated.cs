using Logitar.EventSourcing;
namespace PokeGame.Core.Moves.Events;
public record MoveCreated(PokemonType Type, MoveCategory Category, Slug Key, Accuracy? Accuracy, Power? Power, PowerPoints PowerPoints) : DomainEvent;