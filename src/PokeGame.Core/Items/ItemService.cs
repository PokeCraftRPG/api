using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Items.Commands;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Items.Queries;

namespace PokeGame.Core.Items;

public interface IItemService
{
  Task<CreateOrReplaceItemResult> CreateOrReplaceAsync(CreateOrReplaceItemPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<ItemModel?> UpdateAsync(Guid id, UpdateItemPayload payload, CancellationToken cancellationToken = default);
}

internal class ItemService : IItemService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IItemService, ItemService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceItemCommand, CreateOrReplaceItemResult>, CreateOrReplaceItemCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateItemCommand, ItemModel?>, UpdateItemCommandHandler>();
    services.AddTransient<IQueryHandler<ReadItemQuery, ItemModel?>, ReadItemQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public ItemService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceItemResult> CreateOrReplaceAsync(CreateOrReplaceItemPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceItemCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<ItemModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadItemQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<ItemModel?> UpdateAsync(Guid id, UpdateItemPayload payload, CancellationToken cancellationToken)
  {
    UpdateItemCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
