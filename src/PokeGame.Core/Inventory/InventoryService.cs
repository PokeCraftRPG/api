using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Inventory.Commands;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Inventory.Queries;

namespace PokeGame.Core.Inventory;

public interface IInventoryService
{
  Task<InventoryItemModel> AddAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken = default);
  Task<InventoryItemModel?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken = default);
  Task<InventoryItemModel> RemoveAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken = default);
  Task<SearchResults<InventoryItemModel>> SearchAsync(Guid trainerId, CancellationToken cancellationToken = default);
  Task<InventoryItemModel> UpdateAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken = default);
}

internal class InventoryService : IInventoryService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IInventoryService, InventoryService>();
    services.AddTransient<ICommandHandler<AddInventoryItemCommand, InventoryItemModel>, AddInventoryItemCommandHandler>();
    services.AddTransient<ICommandHandler<RemoveInventoryItemCommand, InventoryItemModel>, RemoveInventoryItemCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateInventoryItemCommand, InventoryItemModel>, UpdateInventoryItemCommandHandler>();
    services.AddTransient<IQueryHandler<ReadInventoryQuery, InventoryItemModel?>, ReadInventoryQueryHandler>();
    services.AddTransient<IQueryHandler<SearchInventoryQuery, SearchResults<InventoryItemModel>>, SearchInventoryQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public InventoryService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<InventoryItemModel> AddAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken)
  {
    AddInventoryItemCommand command = new(trainerId, itemId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<InventoryItemModel?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken)
  {
    ReadInventoryQuery query = new(trainerId, itemId);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<InventoryItemModel> RemoveAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken)
  {
    RemoveInventoryItemCommand command = new(trainerId, itemId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<SearchResults<InventoryItemModel>> SearchAsync(Guid trainerId, CancellationToken cancellationToken)
  {
    SearchInventoryQuery query = new(trainerId);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<InventoryItemModel> UpdateAsync(Guid trainerId, Guid itemId, InventoryQuantityPayload payload, CancellationToken cancellationToken)
  {
    UpdateInventoryItemCommand command = new(trainerId, itemId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}

// TODO(fpion): missing permission checks in commands!
