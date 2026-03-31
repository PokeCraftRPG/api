using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Extensions;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("membership/invitations")]
public class MembershipInvitationController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MembershipInvitationController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpPatch("{id}/accept")]
  public async Task<ActionResult<MembershipInvitationModel?>> AcceptAsync(Guid id, CancellationToken cancellationToken)
  {
    MembershipInvitationModel? invitation = await _membershipService.AcceptInvitationAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("{id}")]
  public async Task<ActionResult<MembershipInvitationModel?>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MembershipInvitationModel? invitation = await _membershipService.ReadInvitationAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost]
  public async Task<ActionResult<MembershipInvitationModel>> SendAsync([FromBody] SendMembershipInvitationPayload payload, CancellationToken cancellationToken)
  {
    MembershipInvitationModel invitation = await _membershipService.SendInvitationAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUri()}/membership/invitations/{invitation.Id}", UriKind.Absolute);
    return Created(location, invitation);
  }
}
