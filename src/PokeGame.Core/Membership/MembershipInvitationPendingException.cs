using Krakenar.Contracts;
using Krakenar.Contracts.Users;
using Logitar;

namespace PokeGame.Core.Membership;

public class MembershipInvitationPendingException : ConflictException
{
  private const string ErrorMessage = "There exist at least one pending, non-expired membership invitation sent to the specified email address.";

  public string EmailAddress
  {
    get => (string)Data[nameof(EmailAddress)]!;
    private set => Data[nameof(EmailAddress)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(EmailAddress)] = EmailAddress;
      return error;
    }
  }

  public MembershipInvitationPendingException(IEmail email)
    : base(BuildMessage(email))
  {
    EmailAddress = email.Address;
  }

  private static string BuildMessage(IEmail email) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(EmailAddress), email.Address)
    .Build();
}
