using Krakenar.Client;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
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
  private readonly ISessionGateway _sessionGateway;
  private readonly ITokenGateway _tokenGateway;
  private readonly IUserGateway _userGateway;

  public IdentityController(
    ErrorSettings errorSettings,
    IIdentityService identityService,
    ILogger<IdentityController> logger,
    ISessionGateway sessionGateway,
    ITokenGateway tokenGateway,
    IUserGateway userGateway)
  {
    _errorSettings = errorSettings;
    _identityService = identityService;
    _logger = logger;
    _sessionGateway = sessionGateway;
    _tokenGateway = tokenGateway;
    _userGateway = userGateway;
  }

  [HttpGet("/profile")]
  [Authorize]
  public async Task<ActionResult<ProfileModel>> GetProfileAsync(CancellationToken cancellationToken)
  {
    User? user = HttpContext.GetUser();
    if (user is null)
    {
      _logger.LogError("{Error}", "An authenticated user is required.");
      return InvalidCredentials();
    }

    if (user.Version < 1)
    {
      user = await _userGateway.FindAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException($"The user 'Id={user.Id}' was not found.");
    }

    ProfileModel profile = new(user);
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
  public async Task<ActionResult> SignOutAsync(bool everywhere, CancellationToken cancellationToken)
  {
    if (everywhere)
    {
      User? user = HttpContext.GetUser();
      if (user is not null)
      {
        await _userGateway.SignOutAsync(user, cancellationToken);
      }
    }
    else
    {
      Session? session = HttpContext.GetSession();
      if (session is not null)
      {
        await _sessionGateway.SignOutAsync(session, cancellationToken);
      }
    }
    HttpContext.SignOut();
    return NoContent();
  }

  [HttpPatch("/profile")]
  [Authorize]
  public async Task<ActionResult<ProfileModel>> UpdateProfileAsync([FromBody] UpdateProfilePayload payload, CancellationToken cancellationToken)
  {
    User? user = HttpContext.GetUser();
    if (user is null)
    {
      _logger.LogError("{Error}", "An authenticated user is required.");
      return InvalidCredentials();
    }

    ProfileModel profile = await _identityService.UpdateProfileAsync(user.Id, payload, cancellationToken);
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
