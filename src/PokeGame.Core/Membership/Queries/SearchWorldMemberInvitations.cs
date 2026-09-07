using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Queries;

internal record SearchWorldMemberInvitationsQuery(Guid WorldId, SearchMemberInvitationsPayload Payload) : IQuery<SearchResults<MemberInvitationDto>?>;

internal class SearchWorldMemberInvitationsQueryHandler : IQueryHandler<SearchWorldMemberInvitationsQuery, SearchResults<MemberInvitationDto>?>
{
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public SearchWorldMemberInvitationsQueryHandler(IMemberInvitationQuerier memberInvitationQuerier, IPermissionService permissionService, IWorldRepository worldRepository)
  {
    _memberInvitationQuerier = memberInvitationQuerier;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<SearchResults<MemberInvitationDto>?> HandleAsync(SearchWorldMemberInvitationsQuery query, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = query.Payload;
    payload.Validate();

    WorldId worldId = new(query.WorldId);
    World? world = await _worldRepository.LoadAsync(worldId, cancellationToken);
    if (world is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.ViewInvitations, world, cancellationToken);

    return await _memberInvitationQuerier.SearchWorldAsync(world, payload, cancellationToken);
  }
}
