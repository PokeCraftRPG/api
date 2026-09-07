using FluentValidation;

namespace PokeGame.Core.Trainers.Models;

public record CreateOrReplaceTrainerPayload
{
  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  // TODO(fpion): License
  // TODO(fpion): Gender
  // TODO(fpion): Money
  // TODO(fpion): Sprite
  // TODO(fpion): User/Member

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceTrainerPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      // TODO(fpion): License
      // TODO(fpion): Gender
      // TODO(fpion): Money
      // TODO(fpion): Sprite
      // TODO(fpion): User/Member
    }
  }
}
