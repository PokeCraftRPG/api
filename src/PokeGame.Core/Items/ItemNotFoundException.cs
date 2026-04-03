using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public class ItemNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified item was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Item
  {
    get => (string)Data[nameof(Item)]!;
    private set => Data[nameof(Item)] = value;
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
      error.Data[nameof(Item)] = Item;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public ItemNotFoundException(WorldId worldId, string item, string propertyName)
    : base(BuildMessage(worldId, item, propertyName))
  {
    WorldId = worldId.ToGuid();
    Item = item;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string item, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Item), item)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
