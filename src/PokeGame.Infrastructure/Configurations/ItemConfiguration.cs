using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class ItemConfiguration : AggregateConfiguration<ItemEntity>, IEntityTypeConfiguration<ItemEntity>
{
  private const int CategoryMaximumLength = 16;

  public override void Configure(EntityTypeBuilder<ItemEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Items), PokemonContext.Schema);
    builder.HasKey(x => x.ItemId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Price });
    builder.HasIndex(x => new { x.WorldId, x.MoveId });

    builder.Property(x => x.Category).HasMaxLength(CategoryMaximumLength).HasConversion(new EnumToStringConverter<ItemCategory>());
    builder.Property(x => x.Key).HasMaxLength(Constants.SlugMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.Sprite).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Items).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Move).WithMany(x => x.TechnicalMachines).OnDelete(DeleteBehavior.Restrict);
  }
}
