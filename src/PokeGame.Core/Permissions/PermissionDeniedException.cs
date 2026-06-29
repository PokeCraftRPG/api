using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Permissions;

public class PermissionDeniedException : ErrorException
{
  private const string ErrorMessage = "The specified permission was denied.";

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
  public string? Resource
  {
    get => (string?)Data[nameof(Resource)];
    private set => Data[nameof(Resource)] = value;
  }

  public override Error Error => new(this.GetErrorCode(), ErrorMessage);

  public PermissionDeniedException(Guid? userId, string action, ResourceIdentifier? resource)
    : base(BuildMessage(userId, action, resource))
  {
    UserId = userId;
    Action = action;
    Resource = resource?.ToString();
  }

  private static string BuildMessage(Guid? userId, string action, ResourceIdentifier? resource) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(UserId), userId, "<null>")
    .AddData(nameof(Action), action)
    .AddData(nameof(Resource), resource, "<null>")
    .Build();
}
