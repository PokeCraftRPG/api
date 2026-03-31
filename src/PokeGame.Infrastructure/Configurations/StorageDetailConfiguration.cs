using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class StorageDetailConfiguration : IEntityTypeConfiguration<StorageDetailEntity>
{
  private const int EntityKindMaximumLength = 16;

  public void Configure(EntityTypeBuilder<StorageDetailEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.StorageDetail), PokemonContext.Schema);
    builder.HasKey(x => x.StorageDetailId);

    builder.HasIndex(x => x.Key).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.EntityKind, x.EntityId });
    builder.HasIndex(x => x.Size);

    builder.Property(x => x.Key).HasMaxLength(StreamId.MaximumLength);
    builder.Property(x => x.EntityKind).HasMaxLength(EntityKindMaximumLength);

    builder.HasOne(x => x.Summary).WithMany(x => x.Detail).OnDelete(DeleteBehavior.Cascade);
  }
}
