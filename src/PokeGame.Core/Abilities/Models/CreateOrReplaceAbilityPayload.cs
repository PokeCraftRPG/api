using FluentValidation;

namespace PokeGame.Core.Abilities.Models;

public record CreateOrReplaceAbilityPayload
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceAbilityPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Key).Key();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());
    }
  }
}
