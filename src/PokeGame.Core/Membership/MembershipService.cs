using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Membership.Queries;

namespace PokeGame.Core.Membership;

public interface IMembershipService
{
  Task<MembershipInvitationModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MembershipInvitationModel> SendInvitationAsync(SendMembershipInvitationPayload payload, CancellationToken cancellationToken = default);
}

internal class MembershipService : IMembershipService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<ICommandHandler<SendMembershipInvitationCommand, MembershipInvitationModel>, SendMembershipInvitationCommandHandler>();
    services.AddTransient<IQueryHandler<ReadMembershipInvitationQuery, MembershipInvitationModel?>, ReadMembershipInvitationQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public MembershipService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<MembershipInvitationModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ReadMembershipInvitationQuery query = new(id);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<MembershipInvitationModel> SendInvitationAsync(SendMembershipInvitationPayload payload, CancellationToken cancellationToken)
  {
    SendMembershipInvitationCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
