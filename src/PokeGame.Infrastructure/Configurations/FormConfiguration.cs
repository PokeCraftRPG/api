using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class FormConfiguration : AggregateConfiguration<FormEntity>, IEntityTypeConfiguration<FormEntity>
{
  public override void Configure(EntityTypeBuilder<FormEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Forms), PokemonContext.Schema);
    builder.HasKey(x => x.FormId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.VarietyId, x.IsDefault });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.IsBattleOnly });
    builder.HasIndex(x => new { x.WorldId, x.IsMega });
    builder.HasIndex(x => new { x.WorldId, x.Height });
    builder.HasIndex(x => new { x.WorldId, x.Weight });
    builder.HasIndex(x => new { x.WorldId, x.PrimaryType });
    builder.HasIndex(x => new { x.WorldId, x.SecondaryType });
    builder.HasIndex(x => new { x.WorldId, x.YieldExperience });

    builder.Property(x => x.Key).HasMaxLength(Slug.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.PrimaryType).HasMaxLength(Constants.PokemonTypeMaximumLength).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.SecondaryType).HasMaxLength(Constants.PokemonTypeMaximumLength).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.SpriteDefault).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.SpriteShiny).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.SpriteAlternative).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.SpriteAlternativeShiny).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Forms).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variety).WithMany(x => x.Forms).OnDelete(DeleteBehavior.Restrict);
  }
}
