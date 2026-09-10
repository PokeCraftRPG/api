using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Inventory.Commands;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Inventory.Queries;

namespace PokeGame.Core.Inventory;

public interface IInventoryService
{
  Task<InventoryItemDto> AdjustAsync(Guid trainerId, Guid itemId, AdjustInventoryItemPayload payload, CancellationToken cancellationToken = default);
  Task<InventoryItemDto?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken = default);
  Task<SearchResults<InventoryItemDto>?> SearchAsync(Guid trainerId, SearchInventoryItemsPayload payload, CancellationToken cancellationToken = default);
  Task<InventoryItemDto> SetAsync(Guid trainerId, Guid itemId, SetInventoryItemPayload payload, CancellationToken cancellationToken = default);
}

internal class InventoryService : IInventoryService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IInventoryService, InventoryService>();
    services.AddTransient<IInventoryManager, InventoryManager>();
    services.AddTransient<ICommandHandler<AdjustInventoryItemCommand, InventoryItemDto>, AdjustInventoryItemCommandHandler>();
    services.AddTransient<ICommandHandler<SetInventoryItemCommand, InventoryItemDto>, SetInventoryItemCommandHandler>();
    services.AddTransient<IQueryHandler<ReadInventoryItemQuery, InventoryItemDto?>, ReadInventoryItemQueryHandler>();
    services.AddTransient<IQueryHandler<SearchInventoryQuery, SearchResults<InventoryItemDto>?>, SearchInventoryQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public InventoryService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<InventoryItemDto> AdjustAsync(Guid trainerId, Guid itemId, AdjustInventoryItemPayload payload, CancellationToken cancellationToken)
  {
    AdjustInventoryItemCommand command = new(trainerId, itemId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<InventoryItemDto?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    ReadInventoryItemQuery query = new(trainerId, itemId);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<InventoryItemDto>?> SearchAsync(Guid trainerId, SearchInventoryItemsPayload payload, CancellationToken cancellationToken)
  {
    SearchInventoryQuery query = new(trainerId, payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<InventoryItemDto> SetAsync(Guid trainerId, Guid itemId, SetInventoryItemPayload payload, CancellationToken cancellationToken)
  {
    SetInventoryItemCommand command = new(trainerId, itemId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
