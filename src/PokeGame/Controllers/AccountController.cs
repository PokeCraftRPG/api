using Krakenar.Client;
using Krakenar.Contracts;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Authentication;
using PokeGame.Core.Accounts;
using PokeGame.Core.Accounts.Models;
using PokeGame.Extensions;
using PokeGame.Models.Account;
using PokeGame.Settings;

namespace PokeGame.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
  private readonly IAccountService _accountService;
  private readonly ErrorSettings _errorSettings;
  private readonly ILogger<AccountController> _logger;
  private readonly IOpenAuthenticationService _openAuthenticationService;
  private readonly ISessionService _sessionService;
  private readonly IUserService _userService;

  public AccountController(
    IAccountService accountService,
    ErrorSettings errorSettings,
    ILogger<AccountController> logger,
    IOpenAuthenticationService openAuthenticationService,
    ISessionService sessionService,
    IUserService userService)
  {
    _accountService = accountService;
    _errorSettings = errorSettings;
    _logger = logger;
    _openAuthenticationService = openAuthenticationService;
    _sessionService = sessionService;
    _userService = userService;
  }

  [HttpPost("/auth/token")]
  public async Task<ActionResult<GetTokenResponse>> GetTokenAsync([FromBody] SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    try
    {
      SignInAccountResult result = await _accountService.SignInAsync(payload, cancellationToken);

      GetTokenResponse response = new(result);
      if (result.Session is not null)
      {
        response.Token = await _openAuthenticationService.GetTokenResponseAsync(result.Session, cancellationToken);
      }
      return Ok(response);
    }
    catch (KrakenarClientException exception)
    {
      if (_errorSettings.ExposeDetail)
      {
        throw;
      }
      return InvalidCredentials(exception);
    }
  }

  [HttpGet("/profile")]
  [Authorize]
  public async Task<ActionResult<User>> GetProfileAsync(CancellationToken cancellationToken)
  {
    User user = HttpContext.GetUser() ?? throw new InvalidOperationException("No user was found in the context.");
    user = await _userService.ReadAsync(user.Id, uniqueName: null, customIdentifier: null, cancellationToken)
      ?? throw new InvalidOperationException($"The user 'Id={user.Id}' was not found.");
    return Ok(user);
  }

  [HttpPost("/sign/in")]
  public async Task<ActionResult<SignInAccountResponse>> SignInAsync([FromBody] SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    try
    {
      SignInAccountResult result = await _accountService.SignInAsync(payload, cancellationToken);
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
      return InvalidCredentials(exception);
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
        await _userService.SignOutAsync(user.Id, cancellationToken);
      }
    }
    else
    {
      Guid? sessionId = HttpContext.GetSessionId();
      if (sessionId.HasValue)
      {
        await _sessionService.SignOutAsync(sessionId.Value, cancellationToken);
      }
    }
    return NoContent();
  }

  private ActionResult InvalidCredentials(KrakenarClientException exception)
  {
    string serializedError = JsonSerializer.Serialize(exception.Error);
    _logger.LogError(exception, "Invalid credentials: {Error}", serializedError);

    Error error = new("InvalidCredentials", "The specified credentials did not match.");
    return Problem(
      detail: error.Message,
      instance: Request.GetDisplayUrl(),
      statusCode: StatusCodes.Status400BadRequest,
      title: "Invalid Credentials",
      type: null,
      extensions: new Dictionary<string, object?> { ["error"] = error });
  }
}
