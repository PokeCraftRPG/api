using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Species.Commands;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Species.Queries;

namespace PokeGame.Core.Species;

public interface ISpeciesService
{
  Task<CreateOrReplaceSpeciesResult> CreateOrReplaceAsync(CreateOrReplaceSpeciesPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> UpdateAsync(Guid id, UpdateSpeciesPayload payload, CancellationToken cancellationToken = default);
}

internal class SpeciesService : ISpeciesService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<ISpeciesService, SpeciesService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>, CreateOrReplaceSpeciesCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateSpeciesCommand, SpeciesModel?>, UpdateSpeciesCommandHandler>();
    services.AddTransient<IQueryHandler<ReadSpeciesQuery, SpeciesModel?>, ReadSpeciesQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public SpeciesService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceSpeciesResult> CreateOrReplaceAsync(CreateOrReplaceSpeciesPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceSpeciesCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<SpeciesModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadSpeciesQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SpeciesModel?> UpdateAsync(Guid id, UpdateSpeciesPayload payload, CancellationToken cancellationToken)
  {
    UpdateSpeciesCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
