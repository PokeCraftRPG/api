using Logitar.CQRS;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Assets.Settings;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Assets.Commands;

internal record UploadAssetCommand(Entity Entity, UploadAssetPayload Payload) : ICommand<AssetDto?>;

internal class UploadAssetCommandHandler : ICommandHandler<UploadAssetCommand, AssetDto?>
{
  private readonly IAssetManager _assetManager;
  private readonly IAssetQuerier _assetQuerier;
  private readonly IAssetRepository _assetRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly AssetsSettings _settings;

  public UploadAssetCommandHandler(
    IAssetManager assetManager,
    IAssetQuerier assetQuerier,
    IAssetRepository assetRepository,
    IContext context,
    IPermissionService permissionService,
    AssetsSettings settings)
  {
    _assetManager = assetManager;
    _assetQuerier = assetQuerier;
    _assetRepository = assetRepository;
    _context = context;
    _permissionService = permissionService;
    _settings = settings;
  }

  public async Task<AssetDto?> HandleAsync(UploadAssetCommand command, CancellationToken cancellationToken)
  {
    UploadAssetPayload payload = command.Payload;
    payload.Validate();

    Stream stream = payload.Stream ?? throw new InvalidOperationException("The stream is required.");

    await _permissionService.CheckAsync(Actions.Upload, cancellationToken);

    AssetMetadata metadata = await _assetManager.ExtractMetadataAsync(stream, cancellationToken);
    if (!_settings.SupportedTypes.TryGetValue(metadata.MimeType, out AssetSettings? settings))
    {
      throw new MediaTypeNotSupportedException(metadata.MimeType);
    }

    AssetFile file = new(Path.GetFileNameWithoutExtension(payload.FileName), settings.Extension, metadata.MimeType, payload.FileSize);
    Asset asset = new(AssetId.NewId(_context.WorldId), settings.Kind, file, metadata.Dimensions, metadata.Duration, _context.ActorId);
    await _assetManager.StoreAsync(asset, stream, cancellationToken);
    await _assetRepository.SaveAsync(asset, cancellationToken);

    return await _assetQuerier.ReadAsync(asset, cancellationToken);
  }
}
