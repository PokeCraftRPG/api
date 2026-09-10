using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Inventory;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("/trainers/{trainerId}/inventory")]
public class InventoryController : ControllerBase
{
  private readonly IInventoryService _inventoryService;

  public InventoryController(IInventoryService inventoryService)
  {
    _inventoryService = inventoryService;
  }

  [HttpPatch("{itemId}")]
  public async Task<ActionResult<InventoryItemDto>> AdjustAsync(Guid trainerId, Guid itemId, [FromBody] AdjustInventoryItemPayload payload, CancellationToken cancellationToken)
  {
    InventoryItemDto item = await _inventoryService.AdjustAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }

  [HttpGet("{itemId}")]
  public async Task<ActionResult<InventoryItemDto>> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    InventoryItemDto? item = await _inventoryService.ReadAsync(trainerId, itemId, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpDelete("{itemId}")]
  public async Task<ActionResult<InventoryItemDto>> RemoveAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    SetInventoryItemPayload payload = new();
    InventoryItemDto item = await _inventoryService.SetAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<InventoryItemDto>>> SearchAsync(Guid trainerId, [FromQuery] SearchInventoryItemsParameters parameters, CancellationToken cancellationToken)
  {
    SearchInventoryItemsPayload payload = parameters.ToPayload();
    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(trainerId, payload, cancellationToken);
    return results is null ? NotFound() : Ok(results);
  }

  [HttpPut("{itemId}")]
  public async Task<ActionResult<InventoryItemDto>> SetAsync(Guid trainerId, Guid itemId, [FromBody] SetInventoryItemPayload payload, CancellationToken cancellationToken)
  {
    InventoryItemDto item = await _inventoryService.SetAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }
}
