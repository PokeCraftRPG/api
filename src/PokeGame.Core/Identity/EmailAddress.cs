using FluentValidation;

namespace PokeGame.Core.Identity;

public sealed class EmailAddress
{
  public const int MaximumLength = byte.MaxValue;

  public string Value { get; }

  public EmailAddress(string value)
  {
    Value = value.Trim().ToLowerInvariant();
    new Validator().ValidateAndThrow(this);
  }

  public static string Format(string value) => value.Trim().ToLowerInvariant();

  public override bool Equals(object? obj) => obj is EmailAddress emailAddress && emailAddress.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<EmailAddress>
  {
    public Validator()
    {
      RuleFor(x => x.Value).EmailAddressValue();
    }
  }
}
