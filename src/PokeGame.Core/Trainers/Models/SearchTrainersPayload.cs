using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Trainers.Models;

public record SearchTrainersPayload : SearchPayload<TrainerSort>
{
  public Gender? Gender { get; set; }
  public Guid? MemberId { get; set; }

  public override void Validate() => new Validator().ValidateQueryAndThrow(this);

  private class Validator : AbstractValidator<SearchTrainersPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<TrainerSort>());

      RuleFor(x => x.Gender).IsInEnum();
    }
  }
}
