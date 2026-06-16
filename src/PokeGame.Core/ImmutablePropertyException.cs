using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public class ImmutablePropertyException<T> : DomainException
{
  private const string ErrorMessage = "The specified property cannot be mutated.";

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
  public T? CurrentValue
  {
    get => (T?)Data[nameof(CurrentValue)];
    private set => Data[nameof(CurrentValue)] = value;
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
      error.Data[nameof(EntityKind)] = EntityKind;
      error.Data[nameof(EntityId)] = EntityId;
      error.Data[nameof(CurrentValue)] = CurrentValue;
      error.Data[nameof(AttemptedValue)] = AttemptedValue;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public ImmutablePropertyException(IEntityProvider provider, T? currentValue, T? attemptedValue, string propertyName)
    : base(BuildMessage(provider, currentValue, attemptedValue, propertyName))
  {
    Entity entity = provider.GetEntity();
    WorldId = entity.WorldId?.EntityId;
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    CurrentValue = currentValue;
    AttemptedValue = attemptedValue;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IEntityProvider provider, T? currentValue, T? attemptedValue, string propertyName)
  {
    Entity entity = provider.GetEntity();
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), entity.WorldId?.EntityId, "<null>")
      .AddData(nameof(EntityKind), entity.Kind)
      .AddData(nameof(EntityId), entity.Id)
      .AddData(nameof(CurrentValue), currentValue, "<null>")
      .AddData(nameof(AttemptedValue), attemptedValue, "<null>")
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
