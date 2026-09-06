using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public abstract class ImmutablePropertyException : ConflictException
{
  protected ImmutablePropertyException(string? message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}

public sealed class ImmutablePropertyException<T> : ImmutablePropertyException
{
  private const string ErrorMessage = "The specified property cannot be changed.";

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
      error.Data[nameof(EntityKind)] = EntityKind;
      error.Data[nameof(EntityId)] = EntityId;
      error.Data[nameof(ExpectedValue)] = ExpectedValue;
      error.Data[nameof(AttemptedValue)] = AttemptedValue;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public ImmutablePropertyException(IEntityProvider provider, T? expectedValue, T? attemptedValue, string propertyName)
    : base(BuildMessage(provider, expectedValue, attemptedValue, propertyName))
  {
    Entity entity = provider.GetEntity();
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    ExpectedValue = expectedValue;
    AttemptedValue = attemptedValue;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IEntityProvider provider, T? expectedValue, T? attemptedValue, string propertyName)
  {
    Entity entity = provider.GetEntity();
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(EntityKind), entity.Kind)
      .AddData(nameof(EntityId), entity.Id)
      .AddData(nameof(ExpectedValue), expectedValue, "<null>")
      .AddData(nameof(AttemptedValue), attemptedValue, "<null>")
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
