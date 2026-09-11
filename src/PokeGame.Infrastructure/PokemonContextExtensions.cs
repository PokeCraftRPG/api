using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Infrastructure;

internal static class PokemonContextExtensions
{
  public static async Task<int> FindAssetIdAsync(this PokemonContext pokemon, AssetId assetId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Assets
      .Where(x => x.StreamId == assetId.Value)
      .Select(x => (int?)x.AssetId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The asset entity 'StreamId={assetId}' was not found.");
  }

  public static async Task<int> FindFormIdAsync(this PokemonContext pokemon, FormId formId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Forms
      .Where(x => x.StreamId == formId.Value)
      .Select(x => (int?)x.FormId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The form entity 'StreamId={formId}' was not found.");
  }

  public static async Task<int> FindItemIdAsync(this PokemonContext pokemon, ItemId itemId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Items
      .Where(x => x.StreamId == itemId.Value)
      .Select(x => (int?)x.ItemId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The item entity 'StreamId={itemId}' was not found.");
  }

  public static async Task<int> FindMoveIdAsync(this PokemonContext pokemon, MoveId moveId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Moves
      .Where(x => x.StreamId == moveId.Value)
      .Select(x => (int?)x.MoveId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The move entity 'StreamId={moveId}' was not found.");
  }

  public static async Task<int> FindRegionIdAsync(this PokemonContext pokemon, RegionId regionId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Regions
      .Where(x => x.StreamId == regionId.Value)
      .Select(x => (int?)x.RegionId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The region entity 'StreamId={regionId}' was not found.");
  }

  public static async Task<int> FindTrainerIdAsync(this PokemonContext pokemon, TrainerId trainerId, CancellationToken cancellationToken = default)
  {
    return await pokemon.Trainers
      .Where(x => x.StreamId == trainerId.Value)
      .Select(x => (int?)x.TrainerId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The trainer entity 'StreamId={trainerId}' was not found.");
  }

  public static async Task<int> FindWorldIdAsync(this PokemonContext pokemon, StreamId streamId, CancellationToken cancellationToken = default)
  {
    WorldId worldId = Entity.Parse(streamId.Value).WorldId ?? throw new ArgumentException("A world identifier is required.", nameof(streamId));
    return await pokemon.Worlds
      .Where(x => x.StreamId == worldId.Value)
      .Select(x => (int?)x.WorldId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");
  }
}
