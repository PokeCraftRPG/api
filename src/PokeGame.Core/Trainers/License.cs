using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Trainers;

public record License
{
  public const int MaximumLength = 16;

  public string Value { get; }
  public long Size => Value.Length;

  public License(string value)
  {
    Value = Normalize(value);
    new Validator().ValidateAndThrow(this);
  }

  public static string Normalize(string value) => value.Trim().ToUpperInvariant();
  public static License? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<License>
  {
    public Validator()
    {
      RuleFor(x => x.Value).License();
    }
  }
}
