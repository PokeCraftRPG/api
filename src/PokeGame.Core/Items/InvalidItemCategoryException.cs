namespace PokeGame.Core.Items;

public sealed class InvalidItemCategoryException : DomainException
{
  public InvalidItemCategoryException(Item item, ItemCategory expectedCategory, string propertyName)
    : base("The item category was not expected.")
  {
    Data["WorldId"] = item.WorldId.EntityId;
    Data["ItemId"] = item.EntityId;
    Data["ExpectedCategory"] = expectedCategory;
    Data["AttemptedCategory"] = item.Category;
    Data["PropertyName"] = propertyName;
  }
}
