using FluentValidation;

namespace PokeGame.Core.Regions;

public sealed class Location
{
  public const int MaximumLength = 100;

  public string Value { get; }

  public Location(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Location? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Location location && location.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Location>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Location();
    }
  }
}
