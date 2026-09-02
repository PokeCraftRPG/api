using Krakenar.Contracts;
using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Permissions;

public sealed class PermissionDeniedException : ErrorException
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
  public Guid? WorldId
  {
    get => (Guid?)Data[nameof(WorldId)];
    private set => Data[nameof(WorldId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(Principal)] = Principal;
      error.Data[nameof(Action)] = Action;
      error.Data[nameof(Resource)] = Resource;
      error.Data[nameof(WorldId)] = WorldId;
      return error;
    }
  }

  public PermissionDeniedException(ActorId? actorId, string action, Entity? entity, WorldId? worldId)
    : base(BuildMessage(actorId, action, entity, worldId))
  {
    Principal = actorId?.Value;
    Action = action;
    Resource = entity?.ToString();
    WorldId = worldId?.EntityId;
  }

  private static string BuildMessage(ActorId? actorId, string action, Entity? entity, WorldId? worldId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(Principal), actorId, "<null>")
    .AddData(nameof(Action), action)
    .AddData(nameof(Resource), entity, "<null>")
    .AddData(nameof(WorldId), worldId?.EntityId, "<null>")
    .Build();
}
