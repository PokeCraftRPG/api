using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Regions.Commands;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Regions.Queries;

namespace PokeGame.Core.Regions;

public interface IRegionService
{
  Task<CreateOrReplaceRegionResult> CreateOrReplaceAsync(CreateOrReplaceRegionPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<RegionDto?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SearchResults<RegionDto>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken = default);
  Task<RegionDto?> UpdateAsync(Guid id, UpdateRegionPayload payload, CancellationToken cancellationToken = default);
}

internal class RegionService : IRegionService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IRegionService, RegionService>();
    services.AddTransient<IRegionManager, RegionManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceRegionCommand, CreateOrReplaceRegionResult>, CreateOrReplaceRegionCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateRegionCommand, RegionDto?>, UpdateRegionCommandHandler>();
    services.AddTransient<IQueryHandler<ReadRegionQuery, RegionDto?>, ReadRegionQueryHandler>();
    services.AddTransient<IQueryHandler<SearchRegionsQuery, SearchResults<RegionDto>>, SearchRegionsQueryHandler>();
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

  public async Task<RegionDto?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadRegionQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<RegionDto>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken)
  {
    SearchRegionsQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<RegionDto?> UpdateAsync(Guid id, UpdateRegionPayload payload, CancellationToken cancellationToken)
  {
    UpdateRegionCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
