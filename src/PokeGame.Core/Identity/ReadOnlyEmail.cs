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
    Address = address.Trim();
    IsVerified = isVerified;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Address;

  private class Validator : AbstractValidator<ReadOnlyEmail>
  {
    public Validator()
    {
      RuleFor(x => x.Address).EmailAddressValue();
    }
  }
}
