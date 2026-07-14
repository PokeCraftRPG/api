using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity.Commands;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public interface IIdentityService
{
  Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken = default);
  Task<ProfileModel> UpdateProfileAsync(Guid userId, UpdateProfilePayload payload, CancellationToken cancellationToken = default);
}

internal class IdentityService : IIdentityService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IIdentityService, IdentityService>();
    services.AddTransient<ICommandHandler<SignInAccountCommand, SignInAccountResult>, SignInAccountCommandHandler>();
    services.AddTransient<ICommandHandler<UpdateAccountProfileCommand, ProfileModel>, UpdateAccountProfileCommandHandler>();
  }

  private readonly ICommandBus _commandBus;

  public IdentityService(ICommandBus commandBus)
  {
    _commandBus = commandBus;
  }

  public async Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    SignInAccountCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<ProfileModel> UpdateProfileAsync(Guid userId, UpdateProfilePayload payload, CancellationToken cancellationToken)
  {
    UpdateAccountProfileCommand command = new(userId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
