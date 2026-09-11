using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Models.Identity;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
  private readonly IIdentityService _identityService;
  private readonly ITokenGateway _tokenGateway;

  public IdentityController(IIdentityService identityService, ITokenGateway tokenGateway)
  {
    _identityService = identityService;
    _tokenGateway = tokenGateway;
  }

  [HttpGet("/profile")]
  [Authorize]
  public async Task<ActionResult<ProfileModel>> GetProfileAsync(CancellationToken cancellationToken)
  {
    ProfileModel profile = await _identityService.ReadProfileAsync(cancellationToken);
    return Ok(profile);
  }

  [HttpPost("/auth/token")]
  public async Task<ActionResult<GetTokenResponse>> GetTokenAsync([FromBody] SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    SignInAccountResult result = await _identityService.SignInAsync(payload, cancellationToken);

    GetTokenResponse response = new(result);
    if (result.Session is not null)
    {
      response.Token = await _tokenGateway.GetResponseAsync(result.Session, cancellationToken);
    }
    return Ok(response);
  }

  [HttpPost("/sign/in")]
  public async Task<ActionResult<SignInAccountResponse>> SignInAsync([FromBody] SignInAccountRequest request, CancellationToken cancellationToken)
  {
    SignInAccountPayload payload = request.ToPayload();
    SignInAccountResult result = await _identityService.SignInAsync(payload, cancellationToken);
    if (result.Session is not null)
    {
      HttpContext.SignIn(result.Session);
    }

    SignInAccountResponse response = new(result);
    return Ok(response);
  }

  [HttpPost("/sign/out")]
  [Authorize]
  [AllowAnonymous]
  public async Task<ActionResult> SignOutAsync(bool everywhere, CancellationToken cancellationToken)
  {
    if (everywhere)
    {
      await _identityService.SignOutAsync(sessionId: null, cancellationToken);
    }
    else
    {
      Guid? sessionId = HttpContext.GetSessionId();
      if (sessionId.HasValue)
      {
        await _identityService.SignOutAsync(sessionId, cancellationToken);
      }
    }
    HttpContext.SignOut();
    return NoContent();
  }

  [HttpPatch("/profile")]
  [Authorize]
  public async Task<ActionResult<ProfileModel>> UpdateProfileAsync([FromBody] UpdateProfilePayload payload, CancellationToken cancellationToken)
  {
    ProfileModel profile = await _identityService.UpdateProfileAsync(payload, cancellationToken);
    return Ok(profile);
  }
}
