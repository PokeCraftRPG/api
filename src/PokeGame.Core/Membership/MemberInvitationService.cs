using Krakenar.Contracts.Search;
using Logitar.CQRS;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Membership.Commands;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Membership.Queries;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationService
{
  Task<MemberInvitationDto?> AcceptAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> DeclineAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SearchResults<MemberInvitationDto>> SearchAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto> SendAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken = default);
}

internal class MemberInvitationService : IMemberInvitationService
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IMemberInvitationService, MemberInvitationService>();
    services.AddTransient<ICommandHandler<AcceptMemberInvitationCommand, MemberInvitationDto?>, AcceptMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<CancelMemberInvitationCommand, MemberInvitationDto?>, CancelMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<DeclineMemberInvitationCommand, MemberInvitationDto?>, DeclineMemberInvitationCommandHandler>();
    services.AddTransient<ICommandHandler<SendMemberInvitationCommand, MemberInvitationDto>, SendMemberInvitationCommandHandler>();
    services.AddTransient<IQueryHandler<ReadMemberInvitationQuery, MemberInvitationDto?>, ReadMemberInvitationQueryHandler>();
    services.AddTransient<IQueryHandler<SearchMemberInvitationsQuery, SearchResults<MemberInvitationDto>>, SearchMemberInvitationsQueryHandler>();
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

  public async Task<SearchResults<MemberInvitationDto>> SearchAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsQuery query = new(payload);
    return await _queryBus.ExecuteAsync(query, cancellationToken);
  }

  public async Task<MemberInvitationDto> SendAsync(SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    SendMemberInvitationCommand command = new(payload);
    return await _commandBus.ExecuteAsync(command, cancellationToken);
  }
}
