namespace PokeGame.Core.Items;

public interface IItemManager
{
  Task<Item> FindAsync(string item, string propertyName, CancellationToken cancellationToken = default);
}

internal class ItemManager : IItemManager
{
  public async Task<Item> FindAsync(string item, string propertyName, CancellationToken cancellationToken)
  {
    throw new NotImplementedException(); // TODO(fpion): implement
  }
}
