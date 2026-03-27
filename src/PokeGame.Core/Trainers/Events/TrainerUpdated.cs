using Logitar.EventSourcing;

namespace PokeGame.Core.Trainers.Events;

public record TrainerUpdated : DomainEvent
{
  public Optional<Name>? Name { get; set; }
  public Optional<Description>? Description { get; set; }

  public TrainerGender? Gender { get; set; }
  public Money? Money { get; set; }

  public Optional<Url>? Sprite { get; set; }
  public Optional<Url>? Url { get; set; }
  public Optional<Notes>? Notes { get; set; }
}
