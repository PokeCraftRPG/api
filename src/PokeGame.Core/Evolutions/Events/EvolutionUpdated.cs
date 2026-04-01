using Logitar.EventSourcing;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Evolutions.Events;

public record EvolutionUpdated : DomainEvent
{
  public Optional<Level>? Level { get; set; }
  public bool? Friendship { get; set; }
  public Optional<PokemonGender?>? Gender { get; set; }
  public Optional<ItemId?>? HeldItemId { get; set; }
  public Optional<MoveId?>? KnownMoveId { get; set; }
  public Optional<Location>? Location { get; set; }
  public Optional<TimeOfDay?>? TimeOfDay { get; set; }
}
