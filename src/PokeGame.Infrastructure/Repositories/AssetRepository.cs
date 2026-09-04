using Logitar.EventSourcing;
using PokeGame.Core.Assets;

namespace PokeGame.Infrastructure.Repositories;

internal class AssetRepository : Repository, IAssetRepository
{
  public AssetRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Asset?> LoadAsync(AssetId id, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Asset>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Asset>> LoadAsync(IEnumerable<AssetId> ids, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Asset>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Asset asset, CancellationToken cancellationToken)
  {
    await base.SaveAsync(asset, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken)
  {
    await base.SaveAsync(assets, cancellationToken);
  }
}
