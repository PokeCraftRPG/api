using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Filters;
using PokeGame.Core.Membership;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld] // TODO(fpion): remove this and ajust the commands accordingly (context → param).
public class MembershipController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MembershipController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpPost("/members/leave")] // TODO(fpion): `POST /worlds/{id}/leave`.
  public async Task<ActionResult> LeaveAsync(CancellationToken cancellationToken)
  {
    await _membershipService.LeaveAsync(cancellationToken);
    return NoContent();
  }

  [HttpPost("/members/{userId}/revoke")] // TODO(fpion): `POST /worlds/{worldId}/members/{userId}/revoke` (ChatGPT).
  public async Task<ActionResult<WorldDto>> RevokeAsync(Guid userId, CancellationToken cancellationToken)
  {
    WorldDto world = await _membershipService.RevokeAsync(userId, cancellationToken);
    return Ok(world);
  }

  [HttpPost("/members/{userId}/transfer-ownership")] // TODO(fpion): `POST /worlds/{id}/ownership` with UserId in the body.
  public async Task<ActionResult> TransferOwnershipAsync(Guid userId, CancellationToken cancellationToken)
  {
    WorldDto world = await _membershipService.TransferOwnershipAsync(userId, cancellationToken);
    return Ok(world);
  }
}
