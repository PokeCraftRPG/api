using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Worlds.Commands;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds;

public interface IWorldService
{
  Task<CreateOrReplaceWorldResult> CreateOrReplaceAsync(CreateOrReplaceWorldPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<WorldModel?> UpdateAsync(Guid id, UpdateWorldPayload payload, CancellationToken cancellationToken = default);
}

internal class WorldService : IWorldService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IWorldService, WorldService>();
    services.AddTransient<IWorldManager, WorldManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceWorldCommand, CreateOrReplaceWorldResult>, CreateOrReplaceWorldCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateWorldCommand, WorldModel?>, UpdateWorldCommandHandler>();
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

  public async Task<WorldModel?> UpdateAsync(Guid id, UpdateWorldPayload payload, CancellationToken cancellationToken)
  {
    UpdateWorldCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
