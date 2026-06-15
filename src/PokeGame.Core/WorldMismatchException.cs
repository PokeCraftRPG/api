using Logitar;

namespace PokeGame.Core;

public class WorldMismatchException : Exception
{
  private const string ErrorMessage = "The specified entities must reside in the same world.";

  public string ExpectedEntity
  {
    get => (string)Data[nameof(ExpectedEntity)]!;
    private set => Data[nameof(ExpectedEntity)] = value;
  }
  public string AttemptedEntity
  {
    get => (string)Data[nameof(AttemptedEntity)]!;
    private set => Data[nameof(AttemptedEntity)] = value;
  }

  public WorldMismatchException(Entity expectedEntity, Entity attemptedEntity)
    : base(BuildMessage(expectedEntity, attemptedEntity))
  {
    ExpectedEntity = expectedEntity.ToString();
    AttemptedEntity = attemptedEntity.ToString();
  }

  private static string BuildMessage(Entity expectedEntity, Entity attemptedEntity) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(ExpectedEntity), expectedEntity)
    .AddData(nameof(AttemptedEntity), attemptedEntity)
    .Build();

  public static void ThrowIfMismatch(IEntityProvider expected, IEntityProvider attempted)
  {
    Entity expectedEntity = expected.GetEntity();
    Entity attemptedEntity = attempted.GetEntity();
    if (expectedEntity.WorldId != attemptedEntity.WorldId)
    {
      throw new WorldMismatchException(expectedEntity, attemptedEntity);
    }
  }
}
