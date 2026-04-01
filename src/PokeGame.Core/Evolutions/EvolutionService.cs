using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Evolutions.Commands;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions;

public interface IEvolutionService
{
  Task<CreateOrReplaceEvolutionResult> CreateOrReplaceAsync(CreateOrReplaceEvolutionPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
}

internal class EvolutionService : IEvolutionService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEvolutionService, EvolutionService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceEvolutionCommand, CreateOrReplaceEvolutionResult>, CreateOrReplaceEvolutionCommandHandler>();
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
}
