using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class InventoryConfiguration : IEntityTypeConfiguration<InventoryEntity>
{
  public void Configure(EntityTypeBuilder<InventoryEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.Inventory), PokemonContext.Schema);
    builder.HasKey(x => new { x.TrainerId, x.ItemId });

    builder.HasIndex(x => x.ItemId);

    builder.HasOne(x => x.Trainer).WithMany(x => x.Inventory).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Item).WithMany(x => x.Inventory).OnDelete(DeleteBehavior.Restrict);
  }
}
