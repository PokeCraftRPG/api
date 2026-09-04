using FluentValidation;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Varieties.Models;

public record SetVarietyMovePayload
{
  public Guid MoveId { get; set; }

  public LearningMethod LearningMethod { get; set; }
  public int? Level { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SetVarietyMovePayload>
  {
    public Validator()
    {
      RuleFor(x => x.LearningMethod).IsInEnum();
      When(x => x.LearningMethod == LearningMethod.LevelUp, () => RuleFor(x => x.Level).NotNull())
        .Otherwise(() => RuleFor(x => x.Level).Null());
      When(x => x.Level.HasValue, () => RuleFor(x => x.Level!.Value).Level());
    }
  }
}
