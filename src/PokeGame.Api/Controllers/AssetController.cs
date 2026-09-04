using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Extensions;
using PokeGame.Api.Filters;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;

namespace PokeGame.Api.Controllers;

[ApiController]
[Authorize]
[RequireWorld]
[Route("assets")]
public class AssetController : ControllerBase
{
  private const long MaximumUploadSize = 4L * 1024 * 1024 * 1024;

  private readonly IAssetService _assetService;

  public AssetController(IAssetService assetService)
  {
    _assetService = assetService;
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<AssetDto>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AssetDto? asset = await _assetService.ReadAsync(id, cancellationToken);
    return asset is null ? NotFound() : Ok(asset);
  }

  [HttpPost]
  [RequestSizeLimit(MaximumUploadSize)]
  [RequestFormLimits(MultipartBodyLengthLimit = MaximumUploadSize)]
  public async Task<ActionResult<AssetDto>> UploadWorldAsync(IFormFile file, CancellationToken cancellationToken)
  {
    using Stream stream = file.OpenReadStream();
    UploadAssetPayload payload = new(file.FileName, file.Length, stream);

    AssetDto? asset = await _assetService.UploadAsync(payload, cancellationToken);
    if (asset is null)
    {
      return NotFound();
    }

    Uri location = new($"{HttpContext.GetBaseUrl()}/assets/{asset.Id}", UriKind.Absolute);
    return Created(location, asset);
  }
}
