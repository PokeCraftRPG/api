using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Regions;

public record Location
{
  public const int MaximumLength = 100;

  public string Value { get; }
  public long Size => Value.Length;

  public Location(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Location? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Location>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Location();
    }
  }
}
