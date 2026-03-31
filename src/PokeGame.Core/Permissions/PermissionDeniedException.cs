using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Permissions;

public class PermissionDeniedException : ErrorException
{
  private const string ErrorMessage = "The specified permission was denied.";

  public Guid? RealmId
  {
    get => (Guid?)Data[nameof(RealmId)];
    private set => Data[nameof(RealmId)] = value;
  }
  public Guid? UserId
  {
    get => (Guid?)Data[nameof(UserId)];
    private set => Data[nameof(UserId)] = value;
  }
  public string Action
  {
    get => (string)Data[nameof(Action)]!;
    private set => Data[nameof(Action)] = value;
  }
  public Guid? WorldId
  {
    get => (Guid?)Data[nameof(WorldId)];
    private set => Data[nameof(WorldId)] = value;
  }
  public string? EntityKind
  {
    get => (string?)Data[nameof(EntityKind)];
    private set => Data[nameof(EntityKind)] = value;
  }
  public Guid? EntityId
  {
    get => (Guid?)Data[nameof(EntityId)];
    private set => Data[nameof(EntityId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(RealmId)] = RealmId;
      error.Data[nameof(UserId)] = UserId;
      error.Data[nameof(Action)] = Action;
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(EntityKind)] = EntityKind;
      error.Data[nameof(EntityId)] = EntityId;
      return error;
    }
  }

  public PermissionDeniedException(UserId? userId, string action, Entity? entity)
    : base(BuildMessage(userId, action, entity))
  {
    RealmId = userId?.RealmId;
    UserId = userId?.EntityId;
    Action = action;
    WorldId = entity?.WorldId?.ToGuid();
    EntityKind = entity?.Kind;
    EntityId = entity?.Id;
  }

  private static string BuildMessage(UserId? userId, string action, Entity? entity) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(RealmId), userId?.RealmId, "<null>")
    .AddData(nameof(UserId), userId?.EntityId, "<null>")
    .AddData(nameof(Action), action)
    .AddData(nameof(WorldId), entity?.WorldId?.ToGuid(), "<null>")
    .AddData(nameof(EntityKind), entity?.Kind, "<null>")
    .AddData(nameof(EntityId), entity?.Id, "<null>")
    .Build();
}
