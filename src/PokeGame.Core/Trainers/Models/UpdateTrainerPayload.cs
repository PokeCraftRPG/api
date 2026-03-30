using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Trainers.Models;

public record UpdateTrainerPayload
{
  public Optional<Guid?>? OwnerId { get; set; }

  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public TrainerGender? Gender { get; set; }
  public int? Money { get; set; }

  public Optional<string>? Sprite { get; set; }
  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateTrainerPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.Gender.HasValue, () => RuleFor(x => x.Gender!.Value).IsInEnum());
      When(x => x.Money.HasValue, () => RuleFor(x => x.Money!.Value).Money());

      When(x => !string.IsNullOrWhiteSpace(x.Sprite?.Value), () => RuleFor(x => x.Sprite!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());
    }
  }
}
