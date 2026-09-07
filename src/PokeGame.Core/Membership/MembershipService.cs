using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership;

public interface IMembershipService
{
  Task LeaveAsync(CancellationToken cancellationToken = default);
  Task<WorldDto?> RevokeAsync(Guid userId, CancellationToken cancellationToken = default);
}

internal class MembershipService : IMembershipService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMembershipService, MembershipService>();
    services.AddTransient<ICommandHandler<LeaveMembershipCommand, Unit>, LeaveMembershipCommandHandler>();
    services.AddTransient<ICommandHandler<RevokeMembershipCommand, WorldDto?>, RevokeMembershipCommandHandler>();
  }

  private readonly ICommandBus _commandBus;

  public MembershipService(ICommandBus commandBus)
  {
    _commandBus = commandBus;
  }

  public async Task LeaveAsync(CancellationToken cancellationToken)
  {
    LeaveMembershipCommand command = new();
    await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<WorldDto?> RevokeAsync(Guid userId, CancellationToken cancellationToken)
  {
    RevokeMembershipCommand command = new(userId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
