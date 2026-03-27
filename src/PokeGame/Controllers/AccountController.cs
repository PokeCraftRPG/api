using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Accounts;
using PokeGame.Core.Accounts.Models;

namespace PokeGame.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
  private readonly IAccountService _accountService;

  public AccountController(IAccountService accountService)
  {
    _accountService = accountService;
  }

  [HttpPost("/sign/in")]
  public async Task<ActionResult<SignInAccountResult>> SignInAsync([FromBody] SignInAccountPayload payload, CancellationToken cancellationToken)
  {
    SignInAccountResult result = await _accountService.SignInAsync(payload, cancellationToken);
    return Ok(result);
  }
}
