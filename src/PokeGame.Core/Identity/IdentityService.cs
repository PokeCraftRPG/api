using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity.Commands;
using PokeGame.Core.Identity.Models;
using PokeGame.Core.Identity.Queries;

namespace PokeGame.Core.Identity;

public interface IIdentityService
{
  Task<SearchResults<SessionModel>> ListActiveSessionsAsync(CancellationToken cancellationToken = default);
  Task<ProfileModel> ReadProfileAsync(CancellationToken cancellationToken = default);
  Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken = default);
  Task<bool> SignOutAsync(Guid? sessionId = null, CancellationToken cancellationToken = default);
  Task<ProfileModel> UpdateProfileAsync(UpdateProfilePayload payload, CancellationToken cancellationToken = default);
}

internal class IdentityService : IIdentityService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IIdentityService, IdentityService>();
    services.AddTransient<ICommandHandler<SignInAccountCommand, SignInAccountResult>, SignInAccountCommandHandler>();
    services.AddTransient<ICommandHandler<SignOutAccountCommand, bool>, SignOutAccountCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateAccountProfileCommand, ProfileModel>, UpdateAccountProfileCommandHandler>();
    services.AddTransient<IQueryHandler<ReadAccountProfileQuery, ProfileModel>, ReadAccountProfileQueryHandler>();
    services.AddTransient<IQueryHandler<ListActiveSessionsQuery, SearchResults<SessionModel>>, ListActiveSessionsQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public IdentityService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<SearchResults<SessionModel>> ListActiveSessionsAsync(CancellationToken cancellationToken)
  {
    ListActiveSessionsQuery query = new();
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<ProfileModel> ReadProfileAsync(CancellationToken cancellationToken)
  {
    ReadAccountProfileQuery query = new();
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    SignInAccountCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<bool> SignOutAsync(Guid? sessionId, CancellationToken cancellationToken)
  {
    SignOutAccountCommand command = new(sessionId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<ProfileModel> UpdateProfileAsync(UpdateProfilePayload payload, CancellationToken cancellationToken)
  {
    UpdateAccountProfileCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
