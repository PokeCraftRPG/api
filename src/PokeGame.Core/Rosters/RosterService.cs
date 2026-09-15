using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Rosters.Commands;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Rosters.Queries;

namespace PokeGame.Core.Rosters;

public interface IRosterService
{
  Task<CreateOrReplaceTagResult> CreateOrReplaceTagAsync(Guid trainerId, CreateOrReplaceTagPayload payload, Guid? tagId = null, CancellationToken cancellationToken = default);
  Task<TagDto?> DeleteTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken = default);
  Task<TagDto?> ReadTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken = default);
  Task<TagDto?> UpdateTagAsync(Guid trainerId, Guid tagId, UpdateTagPayload payload, CancellationToken cancellationToken = default);
}

internal class RosterService : IRosterService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IRosterService, RosterService>();
    services.AddTransient<IRosterManager, RosterManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceTagCommand, CreateOrReplaceTagResult>, CreateOrReplaceTagCommandHandler>();
    services.AddTransient<ICommandHandler<DeleteTagCommand, TagDto?>, DeleteTagCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateTagCommand, TagDto?>, UpdateTagCommandHandler>();
    services.AddTransient<IQueryHandler<ReadTagQuery, TagDto?>, ReadTagQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public RosterService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceTagResult> CreateOrReplaceTagAsync(Guid trainerId, CreateOrReplaceTagPayload payload, Guid? tagId, CancellationToken cancellationToken)
  {
    CreateOrReplaceTagCommand command = new(trainerId, payload, tagId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<TagDto?> DeleteTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken)
  {
    DeleteTagCommand command = new(trainerId, tagId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<TagDto?> ReadTagAsync(Guid trainerId, Guid tagId, CancellationToken cancellationToken)
  {
    ReadTagQuery query = new(trainerId, tagId);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<TagDto?> UpdateTagAsync(Guid trainerId, Guid tagId, UpdateTagPayload payload, CancellationToken cancellationToken)
  {
    UpdateTagCommand command = new(trainerId, tagId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
