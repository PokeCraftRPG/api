using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Trainers.Commands;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Trainers.Queries;

namespace PokeGame.Core.Trainers;

public interface ITrainerService
{
  Task<CreateOrReplaceTrainerResult> CreateOrReplaceAsync(CreateOrReplaceTrainerPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<TrainerModel?> ReadAsync(Guid? id = null, string? license = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SearchResults<TrainerModel>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken = default);
  Task<TrainerModel?> UpdateAsync(Guid id, UpdateTrainerPayload payload, CancellationToken cancellationToken = default);
}

internal class TrainerService : ITrainerService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<ITrainerService, TrainerService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceTrainerCommand, CreateOrReplaceTrainerResult>, CreateOrReplaceTrainerCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateTrainerCommand, TrainerModel?>, UpdateTrainerCommandHandler>();
    services.AddTransient<IQueryHandler<ReadTrainerQuery, TrainerModel?>, ReadTrainerQueryHandler>();
    services.AddTransient<IQueryHandler<SearchTrainersQuery, SearchResults<TrainerModel>>, SearchTrainersQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public TrainerService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceTrainerResult> CreateOrReplaceAsync(CreateOrReplaceTrainerPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceTrainerCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<TrainerModel?> ReadAsync(Guid? id, string? license, string? key, CancellationToken cancellationToken)
  {
    ReadTrainerQuery query = new(id, license, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<TrainerModel>> SearchAsync(SearchTrainersPayload payload, CancellationToken cancellationToken)
  {
    SearchTrainersQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<TrainerModel?> UpdateAsync(Guid id, UpdateTrainerPayload payload, CancellationToken cancellationToken)
  {
    UpdateTrainerCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
