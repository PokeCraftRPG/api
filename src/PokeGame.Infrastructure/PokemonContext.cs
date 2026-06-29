using Microsoft.EntityFrameworkCore;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Infrastructure;

public class PokemonContext : DbContext
{
  public PokemonContext(DbContextOptions<PokemonContext> options) : base(options)
  {
  }

  internal DbSet<HistoryRecord> History => Set<HistoryRecord>();
  internal DbSet<Region> Regions => Set<Region>();
  internal DbSet<World> Worlds => Set<World>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
