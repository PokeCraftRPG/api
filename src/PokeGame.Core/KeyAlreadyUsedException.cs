namespace PokeGame.Core;

public sealed class KeyAlreadyUsedException : ConflictException
{
  public KeyAlreadyUsedException(IEntityProvider provider, Guid conflictId, Key key, string propertyName)
    : base("The specified key is already used.")
  {
    Entity entity = provider.GetEntity();
    Data["WorldId"] = entity.WorldId?.EntityId;
    Data["EntityKind"] = entity.Kind;
    Data["EntityId"] = entity.Id;
    Data["ConflictId"] = conflictId;
    Data["AttemptedKey"] = key.Value;
    Data["PropertyName"] = propertyName;
  }
}
