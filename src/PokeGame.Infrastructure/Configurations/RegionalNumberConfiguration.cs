using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core.Species;
using PokeGame.Infrastructure.Db;

namespace PokeGame.Infrastructure.Configurations;

internal class RegionalNumberConfiguration : IEntityTypeConfiguration<RegionalNumber>
{
  public void Configure(EntityTypeBuilder<RegionalNumber> builder)
  {
    builder.ToTable(nameof(PokemonContext.RegionalNumbers), Schemas.Pokemon);
    builder.HasKey(x => new { x.SpeciesId, x.RegionId });

    builder.HasIndex(x => new { x.RegionId, x.Number }).IsUnique();

    builder.HasOne(x => x.Species).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Region).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Cascade);
  }
}
