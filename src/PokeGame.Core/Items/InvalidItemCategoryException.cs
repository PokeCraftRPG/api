using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Items;

public sealed class InvalidItemCategoryException : DomainException
{
  private const string ErrorMessage = "The item category was not expected.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid ItemId
  {
    get => (Guid)Data[nameof(ItemId)]!;
    private set => Data[nameof(ItemId)] = value;
  }
  public ItemCategory ExpectedCategory
  {
    get => (ItemCategory)Data[nameof(ExpectedCategory)]!;
    private set => Data[nameof(ExpectedCategory)] = value;
  }
  public ItemCategory AttemptedCategory
  {
    get => (ItemCategory)Data[nameof(AttemptedCategory)]!;
    private set => Data[nameof(AttemptedCategory)] = value;
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
      error.Data[nameof(ItemId)] = ItemId;
      error.Data[nameof(ExpectedCategory)] = ExpectedCategory;
      error.Data[nameof(AttemptedCategory)] = AttemptedCategory;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidItemCategoryException(Item item, ItemCategory expectedCategory, string propertyName)
    : base(BuildMessage(item, expectedCategory, propertyName))
  {
    WorldId = item.WorldId.EntityId;
    ItemId = item.EntityId;
    ExpectedCategory = expectedCategory;
    AttemptedCategory = item.Category;
    PropertyName = propertyName;
  }

  private static string BuildMessage(Item item, ItemCategory expectedCategory, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), item.WorldId.EntityId)
    .AddData(nameof(ItemId), item.EntityId)
    .AddData(nameof(ExpectedCategory), expectedCategory)
    .AddData(nameof(AttemptedCategory), item.Category)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
