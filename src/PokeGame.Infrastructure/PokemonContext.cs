using Microsoft.EntityFrameworkCore;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

public class PokemonContext : DbContext
{
  internal const string Schema = "Pokemon";

  public PokemonContext(DbContextOptions<PokemonContext> options) : base(options)
  {
  }

  internal DbSet<AbilityEntity> Abilities => Set<AbilityEntity>();
  internal DbSet<RegionEntity> Regions => Set<RegionEntity>();
  internal DbSet<WorldEntity> Worlds => Set<WorldEntity>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
