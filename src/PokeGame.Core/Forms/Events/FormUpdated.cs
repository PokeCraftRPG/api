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
  // TODO(fpion): Abilities
  // TODO(fpion): BaseStatistics
  // TODO(fpion): Yield
  // TODO(fpion): Sprites

  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Note { get; set; }
}
