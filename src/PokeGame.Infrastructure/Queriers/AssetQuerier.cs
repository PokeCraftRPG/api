using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class AssetQuerier : IAssetQuerier
{
  private readonly IActorService _actors;
  private readonly DbSet<AssetEntity> _assets;

  public AssetQuerier(IActorService actors, PokemonContext pokemon)
  {
    _actors = actors;
    _assets = pokemon.Assets;
  }

  public async Task<AssetDto> ReadAsync(Asset asset, CancellationToken cancellationToken)
  {
    return await ReadAsync(asset.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The asset entity 'StreamId={asset.Id}' was not found.");
  }
  public async Task<AssetDto?> ReadAsync(AssetId id, CancellationToken cancellationToken)
  {
    AssetEntity? asset = await _assets.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .SingleOrDefaultAsync(cancellationToken);

    return asset is null ? null : await MapAsync(asset, cancellationToken);
  }
  public async Task<AssetDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    AssetEntity? asset = await _assets.AsNoTracking()
      .Where(x => x.Id == id)
      .SingleOrDefaultAsync(cancellationToken);

    return asset is null ? null : await MapAsync(asset, cancellationToken);
  }

  private async Task<AssetDto> MapAsync(AssetEntity asset, CancellationToken cancellationToken)
  {
    return (await MapAsync([asset], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<AssetDto>> MapAsync(IEnumerable<AssetEntity> assets, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = assets.SelectMany(asset => asset.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return assets.Select(mapper.ToAsset).ToList().AsReadOnly();
  }
}
