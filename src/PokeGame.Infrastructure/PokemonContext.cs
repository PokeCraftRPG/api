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

  internal DbSet<RegionEntity> Regions => Set<RegionEntity>();
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
