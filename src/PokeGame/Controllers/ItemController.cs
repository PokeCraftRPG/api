using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("items")]
public class ItemController : ControllerBase
{
  private readonly IItemService _itemService;

  public ItemController(IItemService itemService)
  {
    _itemService = itemService;
  }

  [HttpPost]
  public async Task<ActionResult<ItemModel>> CreateAsync([FromBody] CreateOrReplaceItemPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ItemModel>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ItemModel? item = await _itemService.ReadAsync(id, key: null, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<ItemModel>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    ItemModel? item = await _itemService.ReadAsync(id: null, key, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ItemModel>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceItemPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<ItemModel>> UpdateAsync(Guid id, [FromBody] UpdateItemPayload payload, CancellationToken cancellationToken)
  {
    ItemModel? item = await _itemService.UpdateAsync(id, payload, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  private ActionResult<ItemModel> ToActionResult(CreateOrReplaceItemResult result)
  {
    ItemModel item = result.Item;
    if (result.Created)
    {
      Uri location = new($"{HttpContext.GetBaseUri()}/items/{item.Id}", UriKind.Absolute);
      return Created(location, item);
    }
    return Ok(item);
  }
}
