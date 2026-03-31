using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class StorageSummaryConfiguration : AggregateConfiguration<StorageSummaryEntity>, IEntityTypeConfiguration<StorageSummaryEntity>
{
  public override void Configure(EntityTypeBuilder<StorageSummaryEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.StorageSummary), PokemonContext.Schema);
    builder.HasKey(x => x.WorldId);

    builder.HasIndex(x => x.AllocatedBytes);
    builder.HasIndex(x => x.UsedBytes);
    builder.HasIndex(x => x.RemainingBytes);

    builder.HasOne(x => x.World).WithOne(x => x.StorageSummary)
      .HasPrincipalKey<WorldEntity>(x => x.WorldId).HasForeignKey<StorageSummaryEntity>(x => x.WorldId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
