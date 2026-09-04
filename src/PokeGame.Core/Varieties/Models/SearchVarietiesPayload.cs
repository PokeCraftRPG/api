using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Varieties.Models;

public record SearchVarietiesPayload : SearchPayload<VarietySort>
{
  public string? Species { get; set; }
  public bool? IsDefault { get; set; }
  public bool? CanChangeForm { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchVarietiesPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<VarietySort>());

      RuleFor(x => x.Species).MaximumLength(Key.MaximumLength);
    }
  }
}
