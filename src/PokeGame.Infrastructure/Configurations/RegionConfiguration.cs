using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class RegionConfiguration : AggregateConfiguration<RegionEntity>, IEntityTypeConfiguration<RegionEntity>
{
  public override void Configure(EntityTypeBuilder<RegionEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Regions), PokemonContext.Schema);
    builder.HasKey(x => x.RegionId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Summary });

    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
