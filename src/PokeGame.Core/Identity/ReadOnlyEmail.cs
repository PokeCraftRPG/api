using FluentValidation;
using Krakenar.Contracts.Users;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Identity;

public record ReadOnlyEmail : IEmail
{
  public string Address { get; }
  public bool IsVerified { get; }

  public ReadOnlyEmail(string address, bool isVerified = false)
  {
    Address = address;
    IsVerified = isVerified;
  }

  public override string ToString() => Address;

  private class Validator : AbstractValidator<Email>
  {
    public Validator()
    {
      RuleFor(x => x.Address).EmailAddressValue();
    }
  }
}
