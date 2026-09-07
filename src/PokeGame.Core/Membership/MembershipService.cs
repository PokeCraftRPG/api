using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership;

public interface IMembershipService
{
  Task<bool> LeaveAsync(Guid worldId, CancellationToken cancellationToken = default);
  Task<WorldDto?> RevokeAsync(Guid worldId, RevokeMembershipPayload payload, CancellationToken cancellationToken = default);
  Task<WorldDto?> TransferOwnershipAsync(Guid worldId, TransferOwnershipPayload payload, CancellationToken cancellationToken = default);
}

internal class MembershipService : IMembershipService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMembershipService, MembershipService>();
    services.AddTransient<ICommandHandler<LeaveMembershipCommand, bool>, LeaveMembershipCommandHandler>();
    services.AddTransient<ICommandHandler<RevokeMembershipCommand, WorldDto?>, RevokeMembershipCommandHandler>();
    services.AddTransient<ICommandHandler<TransferOwnershipCommand, WorldDto?>, TransferOwnershipCommandHandler>();
  }

  private readonly ICommandBus _commandBus;

  public MembershipService(ICommandBus commandBus)
  {
    _commandBus = commandBus;
  }

  public async Task<bool> LeaveAsync(Guid worldId, CancellationToken cancellationToken)
  {
    LeaveMembershipCommand command = new(worldId);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<WorldDto?> RevokeAsync(Guid worldId, RevokeMembershipPayload payload, CancellationToken cancellationToken)
  {
    RevokeMembershipCommand command = new(worldId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<WorldDto?> TransferOwnershipAsync(Guid worldId, TransferOwnershipPayload payload, CancellationToken cancellationToken)
  {
    TransferOwnershipCommand command = new(worldId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
