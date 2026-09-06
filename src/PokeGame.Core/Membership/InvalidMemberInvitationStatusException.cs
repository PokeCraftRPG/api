using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Membership;

public sealed class InvalidMemberInvitationStatusException : ConflictException
{
  private const string ErrorMessage = "The specified member invitation status is not valid.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid MemberInvitationId
  {
    get => (Guid)Data[nameof(MemberInvitationId)]!;
    private set => Data[nameof(MemberInvitationId)] = value;
  }
  public MemberInvitationStatus Status
  {
    get => (MemberInvitationStatus)Data[nameof(Status)]!;
    private set => Data[nameof(Status)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(MemberInvitationId)] = MemberInvitationId;
      return error;
    }
  }

  public InvalidMemberInvitationStatusException(MemberInvitation invitation) : base(BuildMessage(invitation))
  {
    WorldId = invitation.WorldId.EntityId;
    MemberInvitationId = invitation.EntityId;
    Status = invitation.Status;
  }

  private static string BuildMessage(MemberInvitation invitation) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), invitation.WorldId.EntityId)
    .AddData(nameof(MemberInvitationId), invitation.EntityId)
    .AddData(nameof(Status), invitation.Status)
    .Build();
}
