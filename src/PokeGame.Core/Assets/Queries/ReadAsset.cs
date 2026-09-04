using PokeGame.Core.Assets.Models;
using Logitar.CQRS;

namespace PokeGame.Core.Assets.Queries;

internal record ReadAssetQuery(Guid Id) : IQuery<AssetDto?>;

internal class ReadAssetQueryHandler : IQueryHandler<ReadAssetQuery, AssetDto?>
{
  private readonly IAssetQuerier _assetQuerier;

  public ReadAssetQueryHandler(IAssetQuerier assetQuerier)
  {
    _assetQuerier = assetQuerier;
  }

  public async Task<AssetDto?> HandleAsync(ReadAssetQuery query, CancellationToken cancellationToken)
  {
    return await _assetQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
