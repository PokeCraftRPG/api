using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public class ImmutablePropertyException<T> : DomainException
{
  private const string ErrorMessage = "The specified property cannot be changed.";

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
  public T? ExpectedValue
  {
    get => (T?)Data[nameof(ExpectedValue)];
    private set => Data[nameof(ExpectedValue)] = value;
  }
  public T? AttemptedValue
  {
    get => (T?)Data[nameof(AttemptedValue)];
    private set => Data[nameof(AttemptedValue)] = value;
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
      error.Data[nameof(ExpectedValue)] = ExpectedValue;
      error.Data[nameof(AttemptedValue)] = AttemptedValue;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public ImmutablePropertyException(IResource resource, T? expectedValue, T? attemptedValue, string propertyName)
    : base(BuildMessage(resource, expectedValue, attemptedValue, propertyName))
  {
    ResourceIdentifier identifier = resource.Identifier;
    WorldId = identifier.WorldId;
    ResourceKind = identifier.Kind;
    ResourceId = identifier.Id;
    ExpectedValue = expectedValue;
    AttemptedValue = attemptedValue;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IResource resource, T? expectedValue, T? attemptedValue, string propertyName)
  {
    ResourceIdentifier identifier = resource.Identifier;
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), identifier.WorldId, "<null>")
      .AddData(nameof(ResourceKind), identifier.Kind)
      .AddData(nameof(ResourceId), identifier.Id)
      .AddData(nameof(ExpectedValue), expectedValue, "<null>")
      .AddData(nameof(AttemptedValue), attemptedValue, "<null>")
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
