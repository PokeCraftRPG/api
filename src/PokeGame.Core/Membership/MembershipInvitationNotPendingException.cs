using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Membership;

public class MembershipInvitationNotPendingException : DomainException
{
  private const string ErrorMessage = "The specified membership invitation status is not Pending.";

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
  public MembershipInvitationStatus Status
  {
    get => (MembershipInvitationStatus)Data[nameof(Status)]!;
    private set => Data[nameof(Status)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(MembershipInvitationId)] = MembershipInvitationId;
      error.Data[nameof(Status)] = Status;
      return error;
    }
  }

  public MembershipInvitationNotPendingException(MembershipInvitation invitation)
    : base(BuildMessage(invitation))
  {
    WorldId = invitation.WorldId.ToGuid();
    MembershipInvitationId = invitation.EntityId;
    Status = invitation.Status;
  }

  private static string BuildMessage(MembershipInvitation invitation) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), invitation.WorldId.ToGuid())
    .AddData(nameof(MembershipInvitationId), invitation.EntityId)
    .AddData(nameof(Status), invitation.Status)
    .Build();
}
