using FluentValidation;

namespace PokeGame.Core.Trainers;

public sealed class License
{
  public const int MaximumLength = 16;

  public string Value { get; }

  public License(string value)
  {
    Value = Format(value);
    new Validator().ValidateAndThrow(this);
  }

  public static string Format(string value) => value.Trim().ToUpperInvariant();

  public override bool Equals(object? obj) => obj is License license && license.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<License>
  {
    public Validator()
    {
      RuleFor(x => x.Value).License();
    }
  }
}
