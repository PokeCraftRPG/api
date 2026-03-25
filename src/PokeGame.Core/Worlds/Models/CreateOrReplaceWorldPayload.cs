using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Worlds.Models;

public record CreateOrReplaceWorldPayload
{
  public string Key { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }

  public CreateOrReplaceWorldPayload() : this(string.Empty)
  {
  }

  public CreateOrReplaceWorldPayload(string key)
  {
    Key = key;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceWorldPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());
    }
  }
}
