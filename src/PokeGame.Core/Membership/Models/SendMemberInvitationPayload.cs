using FluentValidation;

namespace PokeGame.Core.Membership.Models;

public record SendMemberInvitationPayload
{
  public string EmailAddress { get; set; } = string.Empty;
  public string Locale { get; set; } = string.Empty;

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SendMemberInvitationPayload>
  {
    public Validator()
    {
      RuleFor(x => x.EmailAddress).EmailAddressValue();
      RuleFor(x => x.Locale).Locale();
    }
  }
}
