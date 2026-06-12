using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public class KeyAlreadyUsedException : ConflictException
{
  private const string ErrorMessage = "The specified key is already used.";

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
  public Guid ConflictingId
  {
    get => (Guid)Data[nameof(ConflictingId)]!;
    private set => Data[nameof(ConflictingId)] = value;
  }
  public string AttemptedKey
  {
    get => (string)Data[nameof(AttemptedKey)]!;
    private set => Data[nameof(AttemptedKey)] = value;
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
      error.Data[nameof(EntityKind)] = EntityKind;
      error.Data[nameof(EntityId)] = EntityId;
      error.Data[nameof(ConflictingId)] = ConflictingId;
      error.Data[nameof(AttemptedKey)] = AttemptedKey;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public KeyAlreadyUsedException(IEntityProvider provider, Guid conflictingId, Slug attemptedKey, string propertyName)
    : this(provider.GetEntity(), conflictingId, attemptedKey.Value, propertyName)
  {
  }

  public KeyAlreadyUsedException(Entity entity, Guid conflictingId, string attemptedKey, string propertyName)
    : base(BuildMessage(entity, conflictingId, attemptedKey, propertyName))
  {
    WorldId = entity.WorldId?.EntityId;
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    ConflictingId = conflictingId;
    AttemptedKey = attemptedKey;
    PropertyName = propertyName;
  }

  private static string BuildMessage(Entity entity, Guid conflictingId, string attemptedKey, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), entity.WorldId?.EntityId, "<null>")
    .AddData(nameof(EntityKind), entity.Kind)
    .AddData(nameof(EntityId), entity.Id)
    .AddData(nameof(ConflictingId), conflictingId)
    .AddData(nameof(AttemptedKey), attemptedKey)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
