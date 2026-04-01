using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Abilities.Commands;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Abilities.Queries;

namespace PokeGame.Core.Abilities;

public interface IAbilityService
{
  Task<CreateOrReplaceAbilityResult> CreateOrReplaceAsync(CreateOrReplaceAbilityPayload payload, Guid? id = null, CancellationToken cancellationToken = default);
  Task<AbilityModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken = default);
  Task<AbilityModel?> UpdateAsync(Guid id, UpdateAbilityPayload payload, CancellationToken cancellationToken = default);
}

internal class AbilityService : IAbilityService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IAbilityService, AbilityService>();
    services.AddTransient<ICommandHandler<CreateOrReplaceAbilityCommand, CreateOrReplaceAbilityResult>, CreateOrReplaceAbilityCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateAbilityCommand, AbilityModel?>, UpdateAbilityCommandHandler>();
    services.AddTransient<IQueryHandler<ReadAbilityQuery, AbilityModel?>, ReadAbilityQueryHandler>();
    services.AddTransient<IQueryHandler<SearchAbilitiesQuery, SearchResults<AbilityModel>>, SearchAbilitiesQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public AbilityService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<CreateOrReplaceAbilityResult> CreateOrReplaceAsync(CreateOrReplaceAbilityPayload payload, Guid? id, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityCommand command = new(payload, id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<AbilityModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadAbilityQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<AbilityModel>> SearchAsync(SearchAbilitiesPayload payload, CancellationToken cancellationToken)
  {
    SearchAbilitiesQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<AbilityModel?> UpdateAsync(Guid id, UpdateAbilityPayload payload, CancellationToken cancellationToken)
  {
    UpdateAbilityCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
