using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class PokemonTagConfiguration : IEntityTypeConfiguration<PokemonTagEntity>
{
  public void Configure(EntityTypeBuilder<PokemonTagEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.PokemonTags), PokemonContext.Schema);
    builder.HasKey(x => new { x.PokemonId, x.TagId });

    builder.HasIndex(x => x.TagId);

    builder.HasOne(x => x.Pokemon).WithMany(x => x.Tags).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Tag).WithMany().OnDelete(DeleteBehavior.Cascade);
  }
}
