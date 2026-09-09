using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Membership;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[Route("members/invitations")]
public class MemberInvitationController : ControllerBase
{
  private const string GetByIdRoute = "GetMemberInvitation";

  private readonly IMemberInvitationService _memberInvitationService;

  public MemberInvitationController(IMemberInvitationService memberInvitationService)
  {
    _memberInvitationService = memberInvitationService;
  }

  [HttpPost("{id}/accept")]
  public async Task<ActionResult<MemberInvitationDto>> AcceptAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.AcceptAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("{id}/cancel")]
  public async Task<ActionResult<MemberInvitationDto>> CancelAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.CancelAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpPost("{id}/decline")]
  public async Task<ActionResult<MemberInvitationDto>> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.DeclineAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpGet("{id}", Name = GetByIdRoute)]
  public async Task<ActionResult<MemberInvitationDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.ReadAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<MemberInvitationDto>>> SearchReceivedAsync([FromQuery] SearchMemberInvitationsParameters parameters, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = parameters.ToPayload();
    SearchResults<MemberInvitationDto> invitations = await _memberInvitationService.SearchReceivedAsync(payload, cancellationToken);
    return Ok(invitations);
  }

  [HttpGet("/worlds/{worldId}/members/invitations")]
  public async Task<ActionResult<SearchResults<MemberInvitationDto>>> SearchWorldAsync(Guid worldId, [FromQuery] SearchMemberInvitationsParameters parameters, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = parameters.ToPayload();
    SearchResults<MemberInvitationDto>? invitations = await _memberInvitationService.SearchWorldAsync(worldId, payload, cancellationToken);
    return invitations is null ? NotFound() : Ok(invitations);
  }

  [HttpPost("/worlds/{worldId}/members/invitations")]
  public async Task<ActionResult<MemberInvitationDto>> SendAsync(Guid worldId, [FromBody] SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.SendAsync(worldId, payload, cancellationToken);
    return invitation is null ? NotFound() : CreatedAtRoute(GetByIdRoute, new { id = invitation.Id }, invitation);
  }
}
