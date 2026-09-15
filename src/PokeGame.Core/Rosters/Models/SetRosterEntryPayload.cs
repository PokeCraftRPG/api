using FluentValidation;

namespace PokeGame.Core.Rosters.Models;

public record SetRosterEntryPayload
{
  public int Priority { get; set; }
  public List<Guid> TagIds { get; set; } = [];

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<SetRosterEntryPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Priority).Priority();
      RuleFor(x => x.TagIds).MaximumCount(10).UniqueCollection();
    }
  }
}
