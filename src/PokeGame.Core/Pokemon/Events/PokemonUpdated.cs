using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonUpdated : DomainEvent
{
  public Optional<Url>? Sprite { get; set; }
  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
