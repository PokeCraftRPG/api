using FluentValidation;

namespace PokeGame.Core.Items.Models;

public record UpdateItemPayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public Optional<int?>? Price { get; set; }
  public Optional<Guid?>? SpriteId { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateItemPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => x.Price?.Value is not null, () => RuleFor(x => x.Price!.Value!.Value).Price());
    }
  }
}
