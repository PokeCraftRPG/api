using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Api.Models.Membership;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("members/invitations")]
public class MemberInvitationController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MemberInvitationController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpPost("{id}/accept")]
  public async Task<ActionResult<MemberInvitationDto>> AcceptAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.AcceptAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("{id}/cancel")]
  public async Task<ActionResult<MemberInvitationDto>> CancelAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.CancelAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("{id}/decline")]
  public async Task<ActionResult<MemberInvitationDto>> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.DeclineAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost]
  public async Task<ActionResult<MemberInvitationDto>> InviteAsync([FromBody] SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    MemberInvitationDto invitation = await _membershipService.InviteAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUrl()}{invitation.Id}", UriKind.Absolute);
    return Created(location, invitation);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<MemberInvitationDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _membershipService.ReadAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<MemberInvitationDto>>> SearchAsync([FromQuery] SearchMemberInvitationsParameters parameters, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = parameters.ToPayload();
    SearchResults<MemberInvitationDto> invitations = await _membershipService.SearchAsync(payload, cancellationToken);
    return Ok(invitations);
  }
}
