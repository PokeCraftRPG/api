using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItemEntity>
{
  public void Configure(EntityTypeBuilder<InventoryItemEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.Inventory), PokemonContext.Schema);
    builder.HasKey(x => new { x.TrainerId, x.ItemId });

    builder.HasIndex(x => x.Quantity);

    builder.HasOne(x => x.Trainer).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Item).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
