using FluentValidation;

namespace PokeGame.Core.Trainers.Models;

public record UpdateTrainerPayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  // TODO(fpion): License
  // TODO(fpion): Gender
  // TODO(fpion): Money
  // TODO(fpion): Sprite
  // TODO(fpion): User/Member

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateTrainerPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      // TODO(fpion): License
      // TODO(fpion): Gender
      // TODO(fpion): Money
      // TODO(fpion): Sprite
      // TODO(fpion): User/Member
    }
  }
}
