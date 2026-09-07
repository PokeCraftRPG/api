using FluentValidation;
using PokeGame.Core.Seo;

namespace PokeGame.Core.Trainers;

public sealed class License
{
  public const int MaximumLength = 16;

  public string Value { get; }

  public License(string value)
  {
    Value = SlugHelper.Format(value);
    new Validator().ValidateAndThrow(this);
  }

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
