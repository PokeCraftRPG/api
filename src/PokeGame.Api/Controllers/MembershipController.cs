using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
public class MembershipController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MembershipController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpPost("/members/invitations/{id}/accept")]
  [RequireWorld]
  public async Task<ActionResult<MemberInvitationDto>> AcceptAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.AcceptAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("/members/invitations/{id}/cancel")]
  [RequireWorld]
  public async Task<ActionResult<MemberInvitationDto>> CancelAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.CancelAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("/members/invitations/{id}/decline")]
  [RequireWorld]
  public async Task<ActionResult<MemberInvitationDto>> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.DeclineAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("/members/invitations")]
  [RequireWorld]
  public async Task<ActionResult<MemberInvitationDto>> InviteAsync([FromBody] SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    MemberInvitationDto invitation = await _membershipService.InviteAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUrl()}/members/invitations/{invitation.Id}", UriKind.Absolute);
    return Created(location, invitation);
  }
}
