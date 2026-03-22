using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Regions.Commands;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Regions.Queries;

namespace PokeGame.Core.Regions;

public interface IRegionService
{
  Task<CreateOrReplaceRegionResult> CreateOrReplaceAsync(CreateOrReplaceRegionPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<RegionModel?> UpdateAsync(Guid id, UpdateRegionPayload payload, CancellationToken cancellationToken = default);
}

internal class RegionService : IRegionService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IRegionService, RegionService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceRegionCommand, CreateOrReplaceRegionResult>, CreateOrReplaceRegionCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateRegionCommand, RegionModel?>, UpdateRegionCommandHandler>();
    services.AddTransient<IQueryHandler<ReadRegionQuery, RegionModel?>, ReadRegionQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public RegionService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceRegionResult> CreateOrReplaceAsync(CreateOrReplaceRegionPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceRegionCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ReadRegionQuery query = new(id);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<RegionModel?> UpdateAsync(Guid id, UpdateRegionPayload payload, CancellationToken cancellationToken)
  {
    UpdateRegionCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
