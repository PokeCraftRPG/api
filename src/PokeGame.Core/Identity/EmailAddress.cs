using FluentValidation;

namespace PokeGame.Core.Identity;

public sealed class EmailAddress
{
  public const int MaximumLength = byte.MaxValue;

  public string Value { get; }

  public EmailAddress(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  private class Validator : AbstractValidator<EmailAddress>
  {
    public Validator()
    {
      RuleFor(x => x.Value).EmailAddressValue();
    }
  }
}
