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
  public ImmutablePropertyException(IEntityProvider provider, T? expectedValue, T? attemptedValue, string propertyName)
    : base("The specified property cannot be changed.")
  {
    Entity entity = provider.GetEntity();
    Data["WorldId"] = entity.WorldId?.EntityId;
    Data["EntityKind"] = entity.Kind;
    Data["EntityId"] = entity.Id;
    Data["ExpectedValue"] = expectedValue;
    Data["AttemptedValue"] = attemptedValue;
    Data["PropertyName"] = propertyName;
  }
}
