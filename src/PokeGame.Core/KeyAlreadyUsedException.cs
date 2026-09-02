using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public sealed class KeyAlreadyUsedException : ConflictException
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
  public Guid ConflictId
  {
    get => (Guid)Data[nameof(ConflictId)]!;
    private set => Data[nameof(ConflictId)] = value;
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
      error.Data[nameof(ConflictId)] = ConflictId;
      error.Data[nameof(AttemptedKey)] = AttemptedKey;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public KeyAlreadyUsedException(IEntityProvider provider, Guid conflictId, Key key, string propertyName)
    : base(BuildMessage(provider, conflictId, key, propertyName))
  {
    Entity entity = provider.GetEntity();
    WorldId = entity.WorldId?.EntityId;
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    ConflictId = conflictId;
    AttemptedKey = key.Value;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IEntityProvider provider, Guid conflictId, Key key, string propertyName)
  {
    Entity entity = provider.GetEntity();
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), entity.WorldId?.EntityId, "<null>")
      .AddData(nameof(EntityKind), entity.Kind)
      .AddData(nameof(EntityId), entity.Id)
      .AddData(nameof(ConflictId), conflictId)
      .AddData(nameof(AttemptedKey), key)
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
