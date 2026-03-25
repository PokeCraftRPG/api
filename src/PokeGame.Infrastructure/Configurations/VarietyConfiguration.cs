using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core;
using PokeGame.Core.Varieties;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class VarietyConfiguration : AggregateConfiguration<VarietyEntity>, IEntityTypeConfiguration<VarietyEntity>
{
  public override void Configure(EntityTypeBuilder<VarietyEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Varieties), PokemonContext.Schema);
    builder.HasKey(x => x.VarietyId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.SpeciesId, x.IsDefault });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Genus });
    builder.HasIndex(x => new { x.WorldId, x.GenderRatio });
    builder.HasIndex(x => new { x.WorldId, x.CanChangeForm });

    builder.Property(x => x.Key).HasMaxLength(Slug.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Genus).HasMaxLength(Genus.MaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Varieties).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Species).WithMany(x => x.Varieties).OnDelete(DeleteBehavior.Restrict);
  }
}
