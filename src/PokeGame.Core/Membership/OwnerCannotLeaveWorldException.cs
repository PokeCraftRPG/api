using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class OwnerCannotLeaveWorldException : ConflictException
{
  private const string ErrorMessage = "The owner cannot leave this world.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid OwnerId
  {
    get => (Guid)Data[nameof(OwnerId)]!;
    private set => Data[nameof(OwnerId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(OwnerId)] = OwnerId;
      return error;
    }
  }

  public OwnerCannotLeaveWorldException(World world)
    : base(BuildMessage(world))
  {
    WorldId = world.EntityId;
    OwnerId = world.OwnerId.EntityId;
  }

  private static string BuildMessage(World world) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), world.EntityId)
    .AddData(nameof(OwnerId), world.OwnerId.EntityId)
    .Build();
}
