using Logitar;

namespace PokeGame.Core;

public sealed class WorldMismatchException : ArgumentException
{
  private const string ErrorMessage = "The attempted entity does not belong to the same world as the entity.";

  public Guid? WorldId
  {
    get => (Guid?)Data[nameof(WorldId)];
    private set => Data[nameof(WorldId)] = value;
  }
  public string EntityKind
  {
    get => (string)Data[nameof(EntityKind)]!;
    private set => Data[nameof(EntityKind)] = value;
  }
  public Guid EntityId
  {
    get => (Guid)Data[nameof(EntityId)]!;
    private set => Data[nameof(EntityId)] = value;
  }

  public Guid? AttemptedWorldId
  {
    get => (Guid?)Data[nameof(AttemptedWorldId)];
    private set => Data[nameof(AttemptedWorldId)] = value;
  }
  public Guid AttemptedEntityId
  {
    get => (Guid)Data[nameof(AttemptedEntityId)]!;
    private set => Data[nameof(AttemptedEntityId)] = value;
  }
  public string AttemptedEntityKind
  {
    get => (string)Data[nameof(AttemptedEntityKind)]!;
    private set => Data[nameof(AttemptedEntityKind)] = value;
  }

  public WorldMismatchException(Entity entity, Entity attempted, string paramName)
    : base(BuildMessage(entity, attempted), paramName)
  {
    WorldId = entity.WorldId?.EntityId;
    EntityKind = entity.Kind;
    EntityId = entity.Id;

    AttemptedWorldId = attempted.WorldId?.EntityId;
    AttemptedEntityKind = attempted.Kind;
    AttemptedEntityId = attempted.Id;
  }

  private static string BuildMessage(Entity entity, Entity attempted) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), entity.WorldId?.EntityId, "<null>")
    .AddData(nameof(EntityKind), entity.Kind)
    .AddData(nameof(EntityId), entity.Id)
    .AddData(nameof(AttemptedWorldId), attempted.WorldId?.EntityId, "<null>")
    .AddData(nameof(AttemptedEntityKind), attempted.Kind)
    .AddData(nameof(AttemptedEntityId), attempted.Id)
    .Build();

  public static void ThrowIfMismatch(IEntityProvider provider, IEntityProvider attemptedProvider, string paramName)
  {
    Entity entity = provider.GetEntity();
    Entity attempted = attemptedProvider.GetEntity();
    if (entity.WorldId != attempted.WorldId)
    {
      throw new WorldMismatchException(entity, attempted, paramName);
    }
  }
}
