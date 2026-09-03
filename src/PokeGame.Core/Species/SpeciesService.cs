using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Species.Commands;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Species.Queries;

namespace PokeGame.Core.Species;

public interface ISpeciesService
{
  Task<CreateOrReplaceSpeciesResult> CreateOrReplaceAsync(CreateOrReplaceSpeciesPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(Guid? id = null, int? number = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(string region, int number, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> RemoveRegionalNumberAsync(Guid speciesId, Guid regionId, CancellationToken cancellationToken = default);
  Task<SearchResults<SpeciesDto>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken = default);
  Task<SpeciesDto> SetRegionalNumberAsync(Guid speciesId, Guid regionId, SetRegionalNumberPayload payload, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> UpdateAsync(Guid id, UpdateSpeciesPayload payload, CancellationToken cancellationToken = default);
}

internal class SpeciesService : ISpeciesService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<ISpeciesService, SpeciesService>();
    services.AddTransient<ISpeciesManager, SpeciesManager>();
    services.AddTransient<ICommandHandler<CreateOrReplaceSpeciesCommand, CreateOrReplaceSpeciesResult>, CreateOrReplaceSpeciesCommandHandler>();
    services.AddTransient<ICommandHandler<RemoveRegionalNumberCommand, SpeciesDto?>, RemoveRegionalNumberCommandHandler>();
    services.AddTransient<ICommandHandler<SetRegionalNumberCommand, SpeciesDto>, SetRegionalNumberCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateSpeciesCommand, SpeciesDto?>, UpdateSpeciesCommandHandler>();
    services.AddTransient<IQueryHandler<ReadRegionalSpeciesQuery, SpeciesDto?>, ReadRegionalSpeciesQueryHandler>();
    services.AddTransient<IQueryHandler<ReadSpeciesQuery, SpeciesDto?>, ReadSpeciesQueryHandler>();
    services.AddTransient<IQueryHandler<SearchSpeciesQuery, SearchResults<SpeciesDto>>, SearchSpeciesQueryHandler>();
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

  public async Task<SpeciesDto?> ReadAsync(Guid? id, int? number, string? key, CancellationToken cancellationToken)
  {
    ReadSpeciesQuery query = new(id, number, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SpeciesDto?> ReadAsync(string region, int number, CancellationToken cancellationToken)
  {
    ReadRegionalSpeciesQuery query = new(region, number);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SpeciesDto?> RemoveRegionalNumberAsync(Guid speciesId, Guid regionId, CancellationToken cancellationToken)
  {
    RemoveRegionalNumberCommand command = new(speciesId, regionId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<SearchResults<SpeciesDto>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken)
  {
    SearchSpeciesQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SpeciesDto> SetRegionalNumberAsync(Guid speciesId, Guid regionId, SetRegionalNumberPayload payload, CancellationToken cancellationToken)
  {
    SetRegionalNumberCommand command = new(speciesId, regionId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<SpeciesDto?> UpdateAsync(Guid id, UpdateSpeciesPayload payload, CancellationToken cancellationToken)
  {
    UpdateSpeciesCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
