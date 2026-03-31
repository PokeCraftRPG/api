using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Membership.Models;

public record SendMembershipInvitationPayload
{
  public string Locale { get; set; }
  public string EmailAddress { get; set; }

  public SendMembershipInvitationPayload() : this(string.Empty, string.Empty)
  {
  }

  public SendMembershipInvitationPayload(string locale, string emailAddress)
  {
    Locale = locale;
    EmailAddress = emailAddress;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SendMembershipInvitationPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Locale).Locale();
      RuleFor(x => x.EmailAddress).EmailAddressValue();
    }
  }
}
