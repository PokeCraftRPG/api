using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public record FormUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public bool? IsBattleOnly { get; set; }
  public bool? IsMega { get; set; }

  public Height? Height { get; set; }
  public Weight? Weight { get; set; }

  public FormTypes? Types { get; set; }
  public FormAbilities? Abilities { get; set; }
  public BaseStatistics? BaseStatistics { get; set; }
  public Yield? Yield { get; set; }
  public Sprites? Sprites { get; set; }

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
