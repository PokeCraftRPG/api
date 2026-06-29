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
  public string ResourceKind
  {
    get => (string)Data[nameof(ResourceKind)]!;
    private set => Data[nameof(ResourceKind)] = value;
  }
  public Guid ResourceId
  {
    get => (Guid)Data[nameof(ResourceId)]!;
    private set => Data[nameof(ResourceId)] = value;
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
      error.Data[nameof(ResourceKind)] = ResourceKind;
      error.Data[nameof(ResourceId)] = ResourceId;
      error.Data[nameof(ConflictId)] = ConflictId;
      error.Data[nameof(AttemptedKey)] = AttemptedKey;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public KeyAlreadyUsedException(IResource resource, Guid conflictId, string attemptedKey, string propertyName)
    : base(BuildMessage(resource, conflictId, attemptedKey, propertyName))
  {
    ResourceIdentifier identifier = resource.Identifier;
    WorldId = identifier.WorldId;
    ResourceKind = identifier.Kind;
    ResourceId = identifier.Id;
    ConflictId = conflictId;
    AttemptedKey = attemptedKey;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IResource resource, Guid conflictId, string attemptedKey, string propertyName)
  {
    ResourceIdentifier identifier = resource.Identifier;
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), identifier.WorldId, "<null>")
      .AddData(nameof(ResourceKind), identifier.Kind)
      .AddData(nameof(ResourceId), identifier.Id)
      .AddData(nameof(ConflictId), conflictId)
      .AddData(nameof(AttemptedKey), attemptedKey)
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
