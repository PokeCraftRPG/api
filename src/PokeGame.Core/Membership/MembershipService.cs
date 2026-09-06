using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership;

public interface IMembershipService
{
  Task<MemberInvitationDto?> AcceptAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> DeclineAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto> InviteAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken = default);
}

internal class MembershipService : IMembershipService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMembershipService, MembershipService>();
    services.AddTransient<ICommandHandler<AcceptMemberInvitationCommand, MemberInvitationDto?>, AcceptMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<CancelMemberInvitationCommand, MemberInvitationDto?>, CancelMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<DeclineMemberInvitationCommand, MemberInvitationDto?>, DeclineMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<SendMemberInvitationCommand, MemberInvitationDto>, SendMemberInvitationCommandHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public MembershipService(ICommandBus commandBus, IQueryBus queryBus)
  {
    _commandBus = commandBus;
    _queryBus = queryBus;
  }

  public async Task<MemberInvitationDto?> AcceptAsync(Guid id, CancellationToken cancellationToken)
  {
    AcceptMemberInvitationCommand command = new(id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MemberInvitationDto?> CancelAsync(Guid id, CancellationToken cancellationToken)
  {
    CancelMemberInvitationCommand command = new(id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MemberInvitationDto?> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    DeclineMemberInvitationCommand command = new(id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MemberInvitationDto> InviteAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    SendMemberInvitationCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
