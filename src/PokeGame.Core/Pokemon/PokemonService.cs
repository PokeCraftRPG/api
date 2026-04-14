using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Pokemon.Commands;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Pokemon.Queries;

namespace PokeGame.Core.Pokemon;

public interface IPokemonService
{
  Task<PokemonModel?> CatchAsync(Guid id, CatchPokemonPayload payload, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ChangeFormAsync(Guid id, ChangePokemonFormPayload payload, CancellationToken cancellationToken = default);
  Task<PokemonModel> CreateAsync(CreatePokemonPayload payload, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReadAsync(Guid? id = null, string? key = null, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReceiveAsync(Guid id, ReceivePokemonPayload payload, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReleaseAsync(Guid id, CancellationToken cancellationToken = default);
  Task<PokemonModel?> UpdateAsync(Guid id, UpdatePokemonPayload payload, CancellationToken cancellationToken = default);
}

internal class PokemonService : IPokemonService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IPokemonService, PokemonService>();
    services.AddTransient<ICommandHandler<CatchPokemonCommand, PokemonModel?>, CatchPokemonCommandHandler>();
    services.AddTransient<ICommandHandler<ChangePokemonFormCommand, PokemonModel?>, ChangePokemonFormCommandHandler>();
    services.AddTransient<ICommandHandler<CreatePokemonCommand, PokemonModel>, CreatePokemonCommandHandler>();
    services.AddTransient<ICommandHandler<ReceivePokemonCommand, PokemonModel?>, ReceivePokemonCommandHandler>();
    services.AddTransient<ICommandHandler<ReleasePokemonCommand, PokemonModel?>, ReleasePokemonCommandHandler>();
    services.AddTransient<ICommandHandler<UpdatePokemonCommand, PokemonModel?>, UpdatePokemonCommandHandler>();
    services.AddTransient<IQueryHandler<ReadPokemonQuery, PokemonModel?>, ReadPokemonQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public PokemonService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<PokemonModel?> CatchAsync(Guid id, CatchPokemonPayload payload, CancellationToken cancellationToken)
  {
    CatchPokemonCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonModel?> ChangeFormAsync(Guid id, ChangePokemonFormPayload payload, CancellationToken cancellationToken)
  {
    ChangePokemonFormCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonModel> CreateAsync(CreatePokemonPayload payload, CancellationToken cancellationToken)
  {
    CreatePokemonCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonModel?> ReadAsync(Guid? id, string? key, CancellationToken cancellationToken)
  {
    ReadPokemonQuery query = new(id, key);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<PokemonModel?> ReceiveAsync(Guid id, ReceivePokemonPayload payload, CancellationToken cancellationToken)
  {
    ReceivePokemonCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonModel?> ReleaseAsync(Guid id, CancellationToken cancellationToken)
  {
    ReleasePokemonCommand command = new(id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<PokemonModel?> UpdateAsync(Guid id, UpdatePokemonPayload payload, CancellationToken cancellationToken)
  {
    UpdatePokemonCommand command = new(id, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
