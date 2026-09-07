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
[RequireWorld] // TODO(fpion): remove this (except for SendAsync)?
[Route("members/invitations")]
public class MemberInvitationController : ControllerBase
{
  private readonly IMemberInvitationService _memberInvitationService;

  public MemberInvitationController(IMemberInvitationService memberInvitationService)
  {
    _memberInvitationService = memberInvitationService;
  }

  [HttpPost("{id}/accept")] // TODO(fpion): won’t work because of RequireWorld.
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

  [HttpPost("{id}/decline")] // TODO(fpion): won’t work because of RequireWorld.
  public async Task<ActionResult<MemberInvitationDto>> DeclineAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.DeclineAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpGet("{id}")] // TODO(fpion): won’t work for the invitee because of RequireWorld.
  public async Task<ActionResult<MemberInvitationDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.ReadAsync(id, cancellationToken);
    return invitation is null ? NotFound() : Ok(invitation);
  }

  [HttpGet] // TODO(fpion): won’t work for the invitee because of RequireWorld.
  public async Task<ActionResult<SearchResults<MemberInvitationDto>>> SearchAsync([FromQuery] SearchMemberInvitationsParameters parameters, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = parameters.ToPayload();
    SearchResults<MemberInvitationDto> invitations = await _memberInvitationService.SearchAsync(payload, cancellationToken);
    return Ok(invitations);
  }

  [HttpPost]
  public async Task<ActionResult<MemberInvitationDto>> SendAsync([FromBody] SendMemberInvitationPayload payload, CancellationToken cancellationToken)
  {
    MemberInvitationDto invitation = await _memberInvitationService.SendAsync(payload, cancellationToken);
    Uri location = new($"{HttpContext.GetBaseUrl()}{invitation.Id}", UriKind.Absolute);
    return Created(location, invitation);
  }
}
