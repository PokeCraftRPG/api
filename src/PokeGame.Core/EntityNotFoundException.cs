namespace PokeGame.Core;

public sealed class EntityNotFoundException : NotFoundException
{

  public EntityNotFoundException(IEntityProvider provider, string propertyName) : base("The specified entity was not found.")
  {
    Entity entity = provider.GetEntity();
    Data["WorldId"] = entity.WorldId?.EntityId;
    Data["EntityKind"] = entity.Kind;
    Data["EntityId"] = entity.Id;
    Data["PropertyName"] = propertyName;
  }
}
