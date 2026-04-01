using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Membership;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("membership")]
public class MembershipController : ControllerBase
{
  private readonly IMembershipService _membershipService;

  public MembershipController(IMembershipService membershipService)
  {
    _membershipService = membershipService;
  }

  [HttpDelete("{userId}")]
  public async Task<ActionResult<WorldModel>> RevokeAsync(Guid userId, CancellationToken cancellationToken)
  {
    WorldModel world = await _membershipService.RevokeAsync(userId, cancellationToken);
    return Ok(world);
  }
}
