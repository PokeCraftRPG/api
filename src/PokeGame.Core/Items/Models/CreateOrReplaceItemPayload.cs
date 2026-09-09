using FluentValidation;

namespace PokeGame.Core.Items.Models;

public record CreateOrReplaceItemPayload
{
  public ItemCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int? Price { get; set; }
  public Guid? SpriteId { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Key();

      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      When(x => x.Price.HasValue, () => RuleFor(x => x.Price!.Value).Price());
    }
  }
}
