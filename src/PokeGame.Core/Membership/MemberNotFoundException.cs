using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class MemberNotFoundException : ConflictException // TODO(fpion): either rename this, or it inherits from NotFoundException.
{
  private const string ErrorMessage = "The specified user is not a member of this world.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid UserId
  {
    get => (Guid)Data[nameof(UserId)]!;
    private set => Data[nameof(UserId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(UserId)] = UserId;
      return error;
    }
  }

  public MemberNotFoundException(World world, UserId userId)
    : base(BuildMessage(world, userId))
  {
    WorldId = world.EntityId;
    UserId = userId.EntityId;
  }

  private static string BuildMessage(World world, UserId userId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), world.EntityId)
    .AddData(nameof(UserId), userId.EntityId)
    .Build();
}
