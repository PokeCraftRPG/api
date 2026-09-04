using PokeGame.Core.Assets.Models;

namespace PokeGame.Core.Assets;

public interface IAssetQuerier
{
  Task<AssetDto> ReadAsync(Asset asset, CancellationToken cancellationToken = default);
  Task<AssetDto?> ReadAsync(AssetId id, CancellationToken cancellationToken = default);
  Task<AssetDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
}
