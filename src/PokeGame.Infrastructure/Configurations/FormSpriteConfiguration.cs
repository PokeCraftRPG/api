using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class FormSpriteConfiguration : IEntityTypeConfiguration<FormSpriteEntity>
{
  public void Configure(EntityTypeBuilder<FormSpriteEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.FormSprites), PokemonContext.Schema);
    builder.HasKey(x => new { x.FormId, x.Kind });

    builder.HasIndex(x => new { x.FormId, x.AssetId }).IsUnique();

    builder.Property(x => x.Kind).HasMaxLength(16).HasConversion(new EnumToStringConverter<FormSpriteKind>());

    builder.HasOne(x => x.Form).WithMany(x => x.Sprites).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Asset).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
