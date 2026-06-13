using Microsoft.AspNetCore.Mvc;
using PokeGame.Models.Index;
using PokeGame.Settings;

namespace PokeGame.Controllers;

[ApiController]
[Route("")]
public class IndexController : ControllerBase
{
  private readonly ApiSettings _apiSettings;

  public IndexController(ApiSettings apiSettings)
  {
    _apiSettings = apiSettings;
  }

  [HttpGet]
  public ActionResult<ApiVersion> Get() => Ok(new ApiVersion(_apiSettings.Title, _apiSettings.Version, _apiSettings.Build));
}
