using Krakenar.Contracts;
using Logitar;
using Logitar.EventSourcing;

namespace PokeGame.Core.Permissions;

public class PermissionDeniedException : ErrorException
{
  private const string ErrorMessage = "The specified permission was denied.";

  public string? Principal
  {
    get => (string?)Data[nameof(Principal)];
    private set => Data[nameof(Principal)] = value;
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

  public PermissionDeniedException(ActorId? actorId, string action, Entity? entity)
    : base(BuildMessage(actorId, action, entity))
  {
    Principal = actorId?.Value;
    Action = action;
    Resource = entity?.ToString();
  }

  private static string BuildMessage(ActorId? actorId, string action, Entity? entity) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(Principal), actorId, "<null>")
    .AddData(nameof(Action), action)
    .AddData(nameof(Resource), entity, "<null>")
    .Build();
}
