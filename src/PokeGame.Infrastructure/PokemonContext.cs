using Microsoft.EntityFrameworkCore;
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
  internal DbSet<MoveEntity> Moves => Set<MoveEntity>();
  internal DbSet<RegionalNumberEntity> RegionalNumbers => Set<RegionalNumberEntity>();
  internal DbSet<RegionEntity> Regions => Set<RegionEntity>();
  internal DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();
  internal DbSet<VarietyEntity> Varieties => Set<VarietyEntity>();
  internal DbSet<WorldEntity> Worlds => Set<WorldEntity>();

  internal async Task<int> FindWorldIdAsync(WorldId id, CancellationToken cancellationToken = default)
  {
    return await Worlds.Where(x => x.StreamId == id.Value)
      .Select(x => (int?)x.WorldId)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The world entity 'StreamId={id}' was not found.");
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
