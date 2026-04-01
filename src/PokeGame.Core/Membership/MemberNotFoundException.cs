using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public class MemberNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified member was not found.";

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
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(UserId)] = UserId;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public MemberNotFoundException(WorldId worldId, Guid userId, string propertyName)
    : base(BuildMessage(worldId, userId, propertyName))
  {
    WorldId = worldId.ToGuid();
    UserId = userId;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, Guid userId, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(UserId), userId)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
