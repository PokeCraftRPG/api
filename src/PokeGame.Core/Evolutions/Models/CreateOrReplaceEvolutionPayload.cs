using FluentValidation;

namespace PokeGame.Core.Evolutions.Models;

public record CreateOrReplaceEvolutionPayload
{
  public Guid SourceId { get; set; }
  public Guid TargetId { get; set; }
  public EvolutionTrigger Trigger { get; set; }

  public int? Level { get; set; }
  public bool Friendship { get; set; }
  public Gender? Gender { get; set; }
  public Guid? ItemId { get; set; }
  public Guid? MoveId { get; set; }
  public string? Location { get; set; }
  public TimeOfDay? TimeOfDay { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceEvolutionPayload>
  {
    public Validator()
    {
      RuleFor(x => x.TargetId).NotEqual(x => x.SourceId);
      RuleFor(x => x.Trigger).IsInEnum();

      When(x => x.Level.HasValue, () => RuleFor(x => x.Level!.Value).Level());
      RuleFor(x => x.Gender).IsInEnum();
      When(x => !string.IsNullOrWhiteSpace(x.Location), () => RuleFor(x => x.Location!).Location());
      RuleFor(x => x.TimeOfDay).IsInEnum();
    }
  }
}
