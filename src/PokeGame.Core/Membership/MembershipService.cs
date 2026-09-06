using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership;

public interface IMembershipService
{
  Task<MemberInvitationDto> InviteAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken = default);
}

internal class MembershipService : IMembershipService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMembershipService, MembershipService>();
    services.AddTransient<ICommandHandler<SendMemberInvitationCommand, MemberInvitationDto>, SendMemberInvitationCommandHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public MembershipService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<MemberInvitationDto> InviteAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    SendMemberInvitationCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
