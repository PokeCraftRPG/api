using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Membership;

public class MembershipInvitationExpiredException : DomainException
{
  private const string ErrorMessage = "The specified membership invitation has expired.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid MembershipInvitationId
  {
    get => (Guid)Data[nameof(MembershipInvitationId)]!;
    private set => Data[nameof(MembershipInvitationId)] = value;
  }
  public DateTime ExpiredOn
  {
    get => (DateTime)Data[nameof(ExpiredOn)]!;
    private set => Data[nameof(ExpiredOn)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(MembershipInvitationId)] = MembershipInvitationId;
      error.Data[nameof(ExpiredOn)] = ExpiredOn;
      return error;
    }
  }

  public MembershipInvitationExpiredException(MembershipInvitation invitation)
    : base(BuildMessage(invitation))
  {
    WorldId = invitation.WorldId.ToGuid();
    MembershipInvitationId = invitation.EntityId;
    ExpiredOn = (invitation.ExpiresOn ?? DateTime.Now).AsUniversalTime();
  }

  private static string BuildMessage(MembershipInvitation invitation) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), invitation.WorldId.ToGuid())
    .AddData(nameof(MembershipInvitationId), invitation.EntityId)
    .AddData(nameof(ExpiredOn), (invitation.ExpiresOn ?? DateTime.Now).AsUniversalTime())
    .Build();
}
