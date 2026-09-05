using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Membership;

public sealed class MemberInvitationExpiredException : ErrorException
{
  private const string ErrorMessage = "The specified member invitation is expired.";

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
      error.Data[nameof(MemberInvitationId)] = MemberInvitationId;
      error.Data[nameof(ExpiredOn)] = ExpiredOn;
      return error;
    }
  }

  public MemberInvitationExpiredException(MemberInvitation invitation) : base(BuildMessage(invitation))
  {
    WorldId = invitation.WorldId.EntityId;
    MemberInvitationId = invitation.EntityId;
    ExpiredOn = invitation.ExpiresOn ?? throw new ArgumentException("The expiration is required.", nameof(invitation));
  }

  private static string BuildMessage(MemberInvitation invitation) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), invitation.WorldId.EntityId)
    .AddData(nameof(MemberInvitationId), invitation.EntityId)
    .AddData(nameof(ExpiredOn), invitation.ExpiresOn)
    .Build();
}
