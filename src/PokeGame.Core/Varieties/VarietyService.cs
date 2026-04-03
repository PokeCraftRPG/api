using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Varieties.Commands;
using PokeGame.Core.Varieties.Models;
using PokeGame.Core.Varieties.Queries;

namespace PokeGame.Core.Varieties;

public interface IVarietyService
{
  Task<CreateOrReplaceVarietyResult> CreateOrReplaceAsync(CreateOrReplaceVarietyPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<VarietyModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SearchResults<VarietyModel>> SearchAsync(SearchVarietiesPayload payload, CancellationToken cancellationToken = default);
  Task<VarietyModel?> UpdateAsync(Guid id, UpdateVarietyPayload payload, CancellationToken cancellationToken = default);
}

internal class VarietyService : IVarietyService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IVarietyService, VarietyService>();
    services.AddTransient<IVarietyManager, VarietyManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceVarietyCommand, CreateOrReplaceVarietyResult>, CreateOrReplaceVarietyCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateVarietyCommand, VarietyModel?>, UpdateVarietyCommandHandler>();
    services.AddTransient<IQueryHandler<ReadVarietyQuery, VarietyModel?>, ReadVarietyQueryHandler>();
    services.AddTransient<IQueryHandler<SearchVarietiesQuery, SearchResults<VarietyModel>>, SearchVarietiesQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public VarietyService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceVarietyResult> CreateOrReplaceAsync(CreateOrReplaceVarietyPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceVarietyCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<VarietyModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadVarietyQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<VarietyModel>> SearchAsync(SearchVarietiesPayload payload, CancellationToken cancellationToken)
  {
    SearchVarietiesQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<VarietyModel?> UpdateAsync(Guid id, UpdateVarietyPayload payload, CancellationToken cancellationToken)
  {
    UpdateVarietyCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
