using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Membership.Queries;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationService
{
  Task<MemberInvitationDto?> AcceptAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
  Task ClaimAsync(UserId userId, EmailAddress emailAddress, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> DeclineAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SearchResults<MemberInvitationDto>> SearchReceivedAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
  Task<SearchResults<MemberInvitationDto>?> SearchWorldAsync(Guid worldId, SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> SendAsync(Guid worldId, SendMemberInvitationPayload payload, CancellationToken cancellationToken = default);
}

internal class MemberInvitationService : IMemberInvitationService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMemberInvitationService, MemberInvitationService>();
    services.AddTransient<ICommandHandler<AcceptMemberInvitationCommand, MemberInvitationDto?>, AcceptMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<ClaimMemberInvitationsCommand, Unit>, ClaimMemberInvitationsCommandHandler>();
    services.AddTransient<ICommandHandler<CancelMemberInvitationCommand, MemberInvitationDto?>, CancelMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<DeclineMemberInvitationCommand, MemberInvitationDto?>, DeclineMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<SendMemberInvitationCommand, MemberInvitationDto?>, SendMemberInvitationCommandHandler>();
    services.AddTransient<IQueryHandler<ReadMemberInvitationQuery, MemberInvitationDto?>, ReadMemberInvitationQueryHandler>();
    services.AddTransient<IQueryHandler<SearchReceivedMemberInvitationsQuery, SearchResults<MemberInvitationDto>>, SearchReceivedMemberInvitationsQueryHandler>();
    services.AddTransient<IQueryHandler<SearchWorldMemberInvitationsQuery, SearchResults<MemberInvitationDto>?>, SearchWorldMemberInvitationsQueryHandler>();
  }

  private readonly ICommandBus _commandBus;
  private readonly IQueryBus _queryBus;

  public MemberInvitationService(ICommandBus commandBus, IQueryBus queryBus)
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

  public async Task ClaimAsync(UserId userId, EmailAddress emailAddress, CancellationToken cancellationToken)
  {
    ClaimMemberInvitationsCommand command = new(userId, emailAddress);
    await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MemberInvitationDto?> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    DeclineMemberInvitationCommand command = new(id);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }

  public async Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ReadMemberInvitationQuery query = new(id);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<MemberInvitationDto>> SearchReceivedAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken)
  {
    SearchReceivedMemberInvitationsQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<SearchResults<MemberInvitationDto>?> SearchWorldAsync(Guid worldId, SearchMemberInvitationsPayload payload, CancellationToken cancellationToken)
  {
    SearchWorldMemberInvitationsQuery query = new(worldId, payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<MemberInvitationDto?> SendAsync(Guid worldId, SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    SendMemberInvitationCommand command = new(worldId, payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
