using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds.Commands;
using PokeGame.Core.Worlds.Models;
using PokeGame.Core.Worlds.Queries;

namespace PokeGame.Core.Worlds;

public interface IWorldService
{
  Task<CreateOrReplaceWorldResult> CreateOrReplaceAsync(CreateOrReplaceWorldPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<WorldModel?> ReadAsync(Guid? id = null, string? slug = null, CancellationToken cancellationToken = default);
}

internal class WorldService : IWorldService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IWorldService, WorldService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>, CreateOrReplaceWorldCommandHandler>();
    services.AddTransient<IQueryHandler<ReadWorldQuery, WorldModel?>, ReadWorldQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public WorldService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceWorldResult> CreateOrReplaceAsync(CreateOrReplaceWorldPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceWorldCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<WorldModel?> ReadAsync(Guid? id, string? slug, CancellationToken cancellationToken)
  {
    ReadWorldQuery query = new(id, slug);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }
}
