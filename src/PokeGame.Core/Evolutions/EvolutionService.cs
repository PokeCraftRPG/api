using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Evolutions.Commands;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Evolutions.Queries;

namespace PokeGame.Core.Evolutions;

public interface IEvolutionService
{
  Task<CreateOrReplaceEvolutionResult> CreateOrReplaceAsync(CreateOrReplaceEvolutionPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SearchResults<EvolutionModel>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> UpdateAsync(Guid id, UpdateEvolutionPayload payload, CancellationToken cancellationToken = default);
}

internal class EvolutionService : IEvolutionService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEvolutionService, EvolutionService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceEvolutionCommand, CreateOrReplaceEvolutionResult>, CreateOrReplaceEvolutionCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateEvolutionCommand, EvolutionModel?>, UpdateEvolutionCommandHandler>();
    services.AddTransient<IQueryHandler<ReadEvolutionQuery, EvolutionModel?>, ReadEvolutionQueryHandler>();
    services.AddTransient<IQueryHandler<SearchEvolutionsQuery, SearchResults<EvolutionModel>>, SearchEvolutionsQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public EvolutionService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceEvolutionResult> CreateOrReplaceAsync(CreateOrReplaceEvolutionPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceEvolutionCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<EvolutionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ReadEvolutionQuery query = new(id);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<EvolutionModel>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken)
  {
    SearchEvolutionsQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<EvolutionModel?> UpdateAsync(Guid id, UpdateEvolutionPayload payload, CancellationToken cancellationToken)
  {
    UpdateEvolutionCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
