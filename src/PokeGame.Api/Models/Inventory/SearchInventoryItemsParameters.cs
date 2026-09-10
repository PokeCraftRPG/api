using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;

namespace PokeGame.Api.Models.Inventory;

public record SearchInventoryItemsParameters : SearchParameters
{
  [FromQuery(Name = "category")]
  public ItemCategory? Category { get; set; }

  public virtual SearchInventoryItemsPayload ToPayload()
  {
    SearchInventoryItemsPayload payload = new();
    payload.Category = Category;
    Fill(payload);
    return payload;
  }
}
