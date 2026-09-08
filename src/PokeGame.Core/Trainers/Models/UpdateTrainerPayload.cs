using FluentValidation;

namespace PokeGame.Core.Trainers.Models;

public record UpdateTrainerPayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public Optional<string>? License { get; set; }
  public Optional<Gender?>? Gender { get; set; }
  public int? Money { get; set; }
  public Optional<Guid?>? SpriteId { get; set; }

  public Optional<Guid?>? MemberId { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateTrainerPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => !string.IsNullOrWhiteSpace(x.License?.Value), () => RuleFor(x => x.License!.Value!).License());
      When(x => x.Gender is not null, () => RuleFor(x => x.Gender!.Value).IsInEnum());
      When(x => x.Money.HasValue, () => RuleFor(x => x.Money!.Value).Money());
    }
  }
}
