using FluentValidation;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Varieties;

public sealed record VarietyMove
{
  public MoveId MoveId { get; }
  public LearningMethod LearningMethod { get; }
  public Level? Level { get; }

  public VarietyMove(MoveId moveId, LearningMethod learningMethod, Level? level = null)
  {
    MoveId = moveId;
    LearningMethod = learningMethod;
    Level = level;
    new Validator().ValidateAndThrow(this);
  }

  private class Validator : AbstractValidator<VarietyMove>
  {
    public Validator()
    {
      RuleFor(x => x.LearningMethod).IsInEnum();
      When(x => x.LearningMethod == LearningMethod.LevelUp, () => RuleFor(x => x.Level).NotNull())
        .Otherwise(() => RuleFor(x => x.Level).Null());
    }
  }
}
