using Logitar.EventSourcing;
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
    builder.HasIndex(x => x.CreatedBy);
    builder.HasIndex(x => x.CreatedOn);
    builder.HasIndex(x => x.UpdatedBy);
    builder.HasIndex(x => x.UpdatedOn);

    builder.Property(x => x.CreatedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.UpdatedBy).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.Species).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Region).WithMany(x => x.RegionalNumbers).OnDelete(DeleteBehavior.Restrict);
  }
}
