using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Api.Models.Item;

public record SearchItemsParameters : SearchParameters
{
  [FromQuery(Name = "category")]
  public ItemCategory? Category { get; set; }

  public virtual SearchItemsPayload ToPayload()
  {
    SearchItemsPayload payload = new();
    payload.Category = Category;
    Fill(payload);
    return payload;
  }
}
