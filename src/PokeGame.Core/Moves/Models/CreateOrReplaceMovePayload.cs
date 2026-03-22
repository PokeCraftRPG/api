using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves.Models;

public record CreateOrReplaceMovePayload
{
  public PokemonType Type { get; set; }
  public MoveCategory Category { get; set; }

  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceMovePayload>
  {
    public Validator()
    {
      RuleFor(x => x.Type).IsInEnum();
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Name).Name();
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());
    }
  }
}
