using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokemon.Commands;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Pokemon.Queries;

namespace PokeGame.Core.Pokemon;

public interface IPokemonService
{
  Task<PokemonDto?> ChangeFormAsync(Guid pokemonId, Guid formId, CancellationToken cancellationToken = default);
  Task<PokemonDto> CreateAsync(CreatePokemonPayload payload, CancellationToken cancellationToken = default);
  Task<PokemonDto?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<PokemonDto?> UpdateAsync(Guid id, UpdatePokemonPayload payload, CancellationToken cancellationToken = default);
}

internal class PokemonService : IPokemonService
{
  public static void Register(IServiceCollection services)
  {
    services.AddSingleton<IPokemonRandomizer, PokemonRandomizer>();
    services.AddTransient<IPokemonService, PokemonService>();
    services.AddTransient<IPokemonManager, PokemonManager>();
    services.AddTransient<ICommandHandler<ChangePokemonFormCommand, PokemonDto?>, ChangePokemonFormCommandHandler>();
    services.AddTransient<ICommandHandler<CreatePokemonCommand, PokemonDto>, CreatePokemonCommandHandler>();
    services.AddTransient<ICommandHandler<UpdatePokemonCommand, PokemonDto?>, UpdatePokemonCommandHandler>();
    services.AddTransient<IQueryHandler<ReadPokemonQuery, PokemonDto?>, ReadPokemonQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public PokemonService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<PokemonDto?> ChangeFormAsync(Guid pokemonId, Guid formId, CancellationToken cancellationToken)
  {
    ChangePokemonFormCommand command = new(pokemonId, formId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonDto> CreateAsync(CreatePokemonPayload payload, CancellationToken cancellationToken)
  {
    CreatePokemonCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonDto?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadPokemonQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<PokemonDto?> UpdateAsync(Guid id, UpdatePokemonPayload payload, CancellationToken cancellationToken)
  {
    UpdatePokemonCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
