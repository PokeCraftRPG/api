using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Item;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("items")]
public class ItemController : ControllerBase
{
  private const string GetByIdRoute = "GetItem";

  private readonly IItemService _itemService;

  public ItemController(IItemService itemService)
  {
    _itemService = itemService;
  }

  [HttpPost]
  public async Task<ActionResult<ItemDto>> CreateAsync([FromBody] CreateOrReplaceItemPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id: null, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<ItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ItemDto? item = await _itemService.ReadAsync(id, key: null, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpGet("key:{key}")]
  public async Task<ActionResult<ItemDto>> ReadAsync(string key, CancellationToken cancellationToken)
  {
    ItemDto? item = await _itemService.ReadAsync(id: null, key, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ItemDto>> ReplaceAsync(Guid id, [FromBody] CreateOrReplaceItemPayload payload, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id, cancellationToken);
    return ToActionResult(result);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<ItemDto>>> SearchAsync([FromQuery] SearchItemsParameters parameters, CancellationToken cancellationToken)
  {
    SearchItemsPayload payload = parameters.ToPayload();
    SearchResults<ItemDto> items = await _itemService.SearchAsync(payload, cancellationToken);
    return Ok(items);
  }

  [HttpPatch("{id}")]
  public async Task<ActionResult<ItemDto>> UpdateAsync(Guid id, [FromBody] UpdateItemPayload payload, CancellationToken cancellationToken)
  {
    ItemDto? item = await _itemService.UpdateAsync(id, payload, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  private ActionResult<ItemDto> ToActionResult(CreateOrReplaceItemResult result) => result.Created
    ? CreatedAtRoute(GetByIdRoute, new { id = result.Item.Id }, result.Item)
    : Ok(result.Item);
}
