namespace PokeGame.Core.Assets;

public interface IAssetRepository
{
  Task<Asset?> LoadAsync(AssetId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Asset>> LoadAsync(IEnumerable<AssetId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Asset asset, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken = default);
}
