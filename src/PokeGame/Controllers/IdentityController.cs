using Krakenar.Client;
using Logitar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;
using PokeGame.Extensions;
using PokeGame.Models.Identity;
using PokeGame.Settings;

namespace PokeGame.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
  private readonly ErrorSettings _errorSettings;
  private readonly IIdentityService _identityService;
  private readonly ILogger<IdentityController> _logger;
  private readonly ITokenGateway _tokenGateway;

  public IdentityController(ErrorSettings errorSettings, IIdentityService identityService, ILogger<IdentityController> logger, ITokenGateway tokenGateway)
  {
    _errorSettings = errorSettings;
    _identityService = identityService;
    _logger = logger;
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
    try
    {
      SignInAccountResult result = await _identityService.SignInAsync(payload, cancellationToken);

      GetTokenResponse response = new(result);
      if (result.Session is not null)
      {
        response.Token = await _tokenGateway.GetResponseAsync(result.Session, cancellationToken);
      }
      return Ok(response);
    }
    catch (KrakenarClientException exception)
    {
      if (_errorSettings.ExposeDetail)
      {
        throw;
      }

      _logger.LogError(exception, "Invalid credentials: {Error}", JsonSerializer.Serialize(exception.Error));
      return InvalidCredentials();
    }
  }

  [HttpPost("/sign/in")]
  public async Task<ActionResult<SignInAccountResponse>> SignInAsync([FromBody] SignInAccountRequest request, CancellationToken cancellationToken)
  {
    try
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
    catch (KrakenarClientException exception)
    {
      if (_errorSettings.ExposeDetail)
      {
        throw;
      }

      _logger.LogError(exception, "Invalid credentials: {Error}", JsonSerializer.Serialize(exception.Error));
      return InvalidCredentials();
    }
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

  private ObjectResult InvalidCredentials()
  {
    InvalidCredentialsError error = new();
    return Problem(
      detail: error.Message,
      instance: Request.GetDisplayUrl(),
      statusCode: StatusCodes.Status400BadRequest,
      title: error.Code.Humanize(),
      type: null,
      extensions: new Dictionary<string, object?> { ["error"] = error });
  }
}
