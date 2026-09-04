namespace PokeGame.Core.Assets;

public interface IAssetManager
{
  Task<AssetMetadata> ExtractMetadataAsync(Stream stream, CancellationToken cancellationToken = default);
  Task StoreAsync(Asset asset, Stream stream, CancellationToken cancellationToken = default);
}
