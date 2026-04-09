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
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(EmailAddress)] = EmailAddress;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public MembershipInvitationPendingException(IEmail email, string propertyName)
    : base(BuildMessage(email, propertyName))
  {
    EmailAddress = email.Address;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IEmail email, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(EmailAddress), email.Address)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
