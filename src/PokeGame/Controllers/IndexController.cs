using Microsoft.AspNetCore.Mvc;
using PokeGame.Models.Index;
using PokeGame.Settings;

namespace PokeGame.Controllers;

[ApiController]
[Route("")]
public class IndexController : ControllerBase
{
  private readonly ApiSettings _settings;

  public IndexController(ApiSettings settings)
  {
    _settings = settings;
  }

  [HttpGet]
  public ActionResult<ApiVersion> Get()
  {
    return Ok(new ApiVersion(_settings.Title, _settings.Version));
  }
}
