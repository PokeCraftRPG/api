using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core;

public sealed class EntityNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified entity was not found.";

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
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public EntityNotFoundException(IEntityProvider provider, string propertyName)
    : base(BuildMessage(provider, propertyName))
  {
    Entity entity = provider.GetEntity();
    WorldId = entity.WorldId?.EntityId;
    EntityKind = entity.Kind;
    EntityId = entity.Id;
    PropertyName = propertyName;
  }

  private static string BuildMessage(IEntityProvider provider, string propertyName)
  {
    Entity entity = provider.GetEntity();
    return new ErrorMessageBuilder(ErrorMessage)
      .AddData(nameof(WorldId), entity.WorldId?.EntityId, "<null>")
      .AddData(nameof(EntityKind), entity.Kind)
      .AddData(nameof(EntityId), entity.Id)
      .AddData(nameof(PropertyName), propertyName)
      .Build();
  }
}
