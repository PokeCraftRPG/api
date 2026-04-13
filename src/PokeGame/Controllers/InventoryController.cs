using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("trainers/{trainerId}/inventory")]
public class InventoryController : ControllerBase
{
  private readonly IInventoryService _inventoryService;

  public InventoryController(IInventoryService inventoryService)
  {
    _inventoryService = inventoryService;
  }

  [HttpPost("{itemId}")]
  public async Task<ActionResult<InventoryItemModel>> AddAsync(Guid trainerId, Guid itemId, int? quantity, CancellationToken cancellationToken)
  {
    InventoryQuantityPayload payload = new(quantity ?? 1);
    InventoryItemModel item = await _inventoryService.AddAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }

  [HttpGet("{itemId}")]
  public async Task<ActionResult<InventoryItemModel>> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    InventoryItemModel? item = await _inventoryService.ReadAsync(trainerId, itemId, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpDelete("{itemId}")]
  public async Task<ActionResult<InventoryItemModel>> RemoveAsync(Guid trainerId, Guid itemId, int? quantity, CancellationToken cancellationToken)
  {
    InventoryQuantityPayload payload = new(quantity ?? int.MaxValue);
    InventoryItemModel item = await _inventoryService.RemoveAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<InventoryItemModel>>> SearchAsync(Guid trainerId, CancellationToken cancellationToken)
  {
    SearchResults<InventoryItemModel> items = await _inventoryService.SearchAsync(trainerId, cancellationToken);
    return Ok(items);
  }

  [HttpPut("{itemId}")]
  public async Task<ActionResult<InventoryItemModel>> UpdateAsync(Guid trainerId, Guid itemId, [FromBody] InventoryQuantityPayload payload, CancellationToken cancellationToken)
  {
    InventoryItemModel item = await _inventoryService.UpdateAsync(trainerId, itemId, payload, cancellationToken);
    return Ok(item);
  }
}
