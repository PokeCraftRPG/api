using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Trainers.Models;

public record CreateOrReplaceTrainerPayload
{
  public Guid? OwnerId { get; set; }

  public string License { get; set; } = string.Empty;

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public TrainerGender Gender { get; set; }
  public int Money { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }

  public CreateOrReplaceTrainerPayload() : this(string.Empty, string.Empty)
  {
  }

  public CreateOrReplaceTrainerPayload(string license, string key)
  {
    License = license;
    Key = key;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceTrainerPayload>
  {
    public Validator()
    {
      RuleFor(x => x.License).License();

      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      RuleFor(x => x.Gender).IsInEnum();
      RuleFor(x => x.Money).Money();

      When(x => !string.IsNullOrWhiteSpace(x.Sprite), () => RuleFor(x => x.Sprite!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());
    }
  }
}
