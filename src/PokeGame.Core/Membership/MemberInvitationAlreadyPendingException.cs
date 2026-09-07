using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

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

  public MemberInvitationAlreadyPendingException(World world, MemberInvitationId invitationId)
    : base(BuildMessage(world, invitationId))
  {
    WorldId = world.EntityId;
    InvitationId = invitationId.EntityId;
  }

  private static string BuildMessage(World world, MemberInvitationId invitationId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), world.EntityId)
    .AddData(nameof(InvitationId), invitationId.EntityId)
    .Build();
}
