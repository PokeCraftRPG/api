using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeGame.Infrastructure.Configurations;

internal class HistoryRecordConfiguration : IEntityTypeConfiguration<HistoryRecord>
{
  public void Configure(EntityTypeBuilder<HistoryRecord> builder)
  {
    builder.ToTable(nameof(PokemonContext.History));
    builder.HasKey(x => x.HistoryRecordId);

    builder.HasIndex(x => x.EventId).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.ResourceKind, x.ResourceId });
    builder.HasIndex(x => x.Version);
    builder.HasIndex(x => x.OccurredOn);
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.EventType);

    builder.Property(x => x.ResourceKind).HasMaxLength(byte.MaxValue);
    builder.Property(x => x.EventType).HasMaxLength(byte.MaxValue);
  }
}
