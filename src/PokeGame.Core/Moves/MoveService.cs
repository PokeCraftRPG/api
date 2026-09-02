using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Moves.Commands;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Moves.Queries;

namespace PokeGame.Core.Moves;

public interface IMoveService
{
  Task<CreateOrReplaceMoveResult> CreateOrReplaceAsync(CreateOrReplaceMovePayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SearchResults<MoveDto>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken = default);
  Task<MoveDto?> UpdateAsync(Guid id, UpdateMovePayload payload, CancellationToken cancellationToken = default);
}

internal class MoveService : IMoveService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMoveService, MoveService>();
    services.AddTransient<IMoveManager, MoveManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceMoveCommand, CreateOrReplaceMoveResult>, CreateOrReplaceMoveCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateMoveCommand, MoveDto?>, UpdateMoveCommandHandler>();
    services.AddTransient<IQueryHandler<ReadMoveQuery, MoveDto?>, ReadMoveQueryHandler>();
    services.AddTransient<IQueryHandler<SearchMovesQuery, SearchResults<MoveDto>>, SearchMovesQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public MoveService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceMoveResult> CreateOrReplaceAsync(CreateOrReplaceMovePayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceMoveCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MoveDto?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadMoveQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<MoveDto>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken)
  {
    SearchMovesQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<MoveDto?> UpdateAsync(Guid id, UpdateMovePayload payload, CancellationToken cancellationToken)
  {
    UpdateMoveCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
