using FluentValidation;

namespace PokeGame.Core.Evolutions.Models;

public record UpdateEvolutionPayload
{
  public Optional<byte?>? Level { get; set; }
  public bool? Friendship { get; set; }
  public Optional<Gender?>? Gender { get; set; }
  public Optional<Guid?>? ItemId { get; set; }
  public Optional<Guid?>? MoveId { get; set; }
  public Optional<string>? Location { get; set; }
  public Optional<TimeOfDay?>? TimeOfDay { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateEvolutionPayload>
  {
    public Validator()
    {
      When(x => x.Level?.Value is not null, () => RuleFor(x => x.Level!.Value!.Value).Level());
      When(x => x.Gender is not null, () => RuleFor(x => x.Gender!.Value).IsInEnum());
      When(x => !string.IsNullOrWhiteSpace(x.Location?.Value), () => RuleFor(x => x.Location!.Value!).Location());
      When(x => x.TimeOfDay is not null, () => RuleFor(x => x.TimeOfDay!.Value).IsInEnum());
    }
  }
}
