using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

public class PokemonContext : DbContext
{
  internal const string Schema = "Pokemon";

  public PokemonContext(DbContextOptions<PokemonContext> options) : base(options)
  {
  }

  internal DbSet<AbilityEntity> Abilities => Set<AbilityEntity>();
  internal DbSet<AssetEntity> Assets => Set<AssetEntity>();
  internal DbSet<MoveEntity> Moves => Set<MoveEntity>();
  internal DbSet<RegionalNumberEntity> RegionalNumbers => Set<RegionalNumberEntity>();
  internal DbSet<RegionEntity> Regions => Set<RegionEntity>();
  internal DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();
  internal DbSet<VarietyEntity> Varieties => Set<VarietyEntity>();
  internal DbSet<VarietyMoveEntity> VarietyMoves => Set<VarietyMoveEntity>();
  internal DbSet<WorldEntity> Worlds => Set<WorldEntity>();

  internal async Task<int> FindWorldIdAsync(StreamId streamId, CancellationToken cancellationToken = default)
  {
    WorldId worldId = Entity.Parse(streamId.Value).WorldId ?? throw new ArgumentException("A world identifier is required.", nameof(streamId));
    return await Worlds.Where(x => x.StreamId == worldId.Value)
      .Select(x => (int?)x.WorldId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The world entity 'StreamId={worldId}' was not found.");
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
