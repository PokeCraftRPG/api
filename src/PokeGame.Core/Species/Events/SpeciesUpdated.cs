using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }

  public Friendship? BaseFriendship { get; set; }
  public CatchRate? CatchRate { get; set; }
  public GrowthRate? GrowthRate { get; set; }

  public EggCycles? EggCycles { get; set; }
  public EggGroups? EggGroups { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
