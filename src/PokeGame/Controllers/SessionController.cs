using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Controllers;

[ApiController]
[Authorize]
[Route("sessions")]
public class SessionController : ControllerBase
{
  private readonly IIdentityService _identityService;

  public SessionController(IIdentityService identityService)
  {
    _identityService = identityService;
  }

  [HttpGet]
  public async Task<ActionResult<SearchResults<SessionModel>>> SearchAsync(CancellationToken cancellationToken)
  {
    SearchResults<SessionModel> sessions = await _identityService.ListActiveSessionsAsync(cancellationToken);
    return Ok(sessions);
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> SignOutAsync(Guid id, CancellationToken cancellationToken)
  {
    bool found = await _identityService.SignOutAsync(id, cancellationToken);
    return found ? NoContent() : NotFound();
  }
}
