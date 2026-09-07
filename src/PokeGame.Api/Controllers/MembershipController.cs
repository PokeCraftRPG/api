using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[Route("worlds/{worldId}")]
public class MembershipController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MembershipController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpPost("leave")]
  public async Task<ActionResult> LeaveAsync(Guid worldId, CancellationToken cancellationToken)
  {
    bool found = await _membershipService.LeaveAsync(worldId, cancellationToken);
    return found ? NoContent() : NotFound();
  }

  [HttpPost("revoke")]
  public async Task<ActionResult<WorldDto>> RevokeAsync(Guid worldId, [FromBody] RevokeMembershipPayload payload, CancellationToken cancellationToken)
  {
    WorldDto? world = await _membershipService.RevokeAsync(worldId, payload, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }

  [HttpPost("ownership")]
  public async Task<ActionResult> TransferOwnershipAsync(Guid worldId, [FromBody] TransferOwnershipPayload payload, CancellationToken cancellationToken)
  {
    WorldDto? world = await _membershipService.TransferOwnershipAsync(worldId, payload, cancellationToken);
    return world is null ? NotFound() : Ok(world);
  }
}
