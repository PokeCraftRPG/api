using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class ItemConfiguration : AggregateConfiguration<ItemEntity>, IEntityTypeConfiguration<ItemEntity>
{
  public override void Configure(EntityTypeBuilder<ItemEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Items), PokemonContext.Schema);
    builder.HasKey(x => x.ItemId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Summary });
    builder.HasIndex(x => new { x.WorldId, x.Price });
    builder.HasIndex(x => new { x.WorldId, x.Weight });

    builder.Property(x => x.Category).HasMaxLength(16).HasConversion(new EnumToStringConverter<ItemCategory>());
    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Sprite).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
