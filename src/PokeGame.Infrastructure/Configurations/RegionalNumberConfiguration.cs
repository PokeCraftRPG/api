using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class RegionalNumberConfiguration : IEntityTypeConfiguration<RegionalNumberEntity>
{
  public void Configure(EntityTypeBuilder<RegionalNumberEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.RegionalNumbers), PokemonContext.Schema);
    builder.HasKey(x => new { x.SpeciesId, x.RegionId });

    builder.HasIndex(x => new { x.RegionId, x.Number }).IsUnique();

    builder.HasOne(x => x.Species).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Region).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Cascade);
  }
}
