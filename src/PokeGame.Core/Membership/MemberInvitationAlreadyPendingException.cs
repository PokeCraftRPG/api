using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Membership;

public sealed class MemberInvitationAlreadyPendingException : ConflictException
{
  private const string ErrorMessage = "A pending member invitation already exists and has not expired.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid InvitationId
  {
    get => (Guid)Data[nameof(InvitationId)]!;
    private set => Data[nameof(InvitationId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(InvitationId)] = InvitationId;
      return error;
    }
  }

  public MemberInvitationAlreadyPendingException(MemberInvitationId invitationId)
    : base(BuildMessage(invitationId))
  {
    WorldId = invitationId.WorldId.EntityId;
    InvitationId = invitationId.EntityId;
  }

  private static string BuildMessage(MemberInvitationId invitationId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), invitationId.WorldId.EntityId)
    .AddData(nameof(InvitationId), invitationId.EntityId)
    .Build();
}
