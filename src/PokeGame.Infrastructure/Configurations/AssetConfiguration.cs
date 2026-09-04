using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Assets;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class AssetConfiguration : AggregateConfiguration<AssetEntity>, IEntityTypeConfiguration<AssetEntity>
{
  public override void Configure(EntityTypeBuilder<AssetEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Assets), PokemonContext.Schema);
    builder.HasKey(x => x.AssetId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Kind });
    builder.HasIndex(x => new { x.WorldId, x.FileName });
    builder.HasIndex(x => new { x.WorldId, x.FileExtension });
    builder.HasIndex(x => new { x.WorldId, x.FileMimeType });
    builder.HasIndex(x => new { x.WorldId, x.FileSize });
    builder.HasIndex(x => new { x.WorldId, x.Width });
    builder.HasIndex(x => new { x.WorldId, x.Height });
    builder.HasIndex(x => new { x.WorldId, x.Duration });

    builder.Property(x => x.Kind).HasMaxLength(8).HasConversion(new EnumToStringConverter<AssetKind>());
    builder.Property(x => x.FileName).HasMaxLength(AssetFile.NameMaximumLength);
    builder.Property(x => x.FileExtension).HasMaxLength(AssetFile.ExtensionMaximumLength);
    builder.Property(x => x.FileMimeType).HasMaxLength(AssetFile.MimeTypeMaximumLength);

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
