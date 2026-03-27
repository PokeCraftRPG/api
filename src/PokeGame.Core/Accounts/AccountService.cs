using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Accounts.Commands;
using PokeGame.Core.Accounts.Models;

namespace PokeGame.Core.Accounts;

public interface IAccountService
{
  Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken = default);
}

internal class AccountService : IAccountService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IAccountService, AccountService>();
    services.AddTransient<ICommandHandler<SignInAccountCommand, SignInAccountResult>, SignInAccountCommandHandler>();
  }

  private readonly ICommandBus _commandBus;

  public AccountService(ICommandBus commandBus)
  {
    _commandBus = commandBus;
  }

  public async Task<SignInAccountResult> SignInAsync(SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    SignInAccountCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
