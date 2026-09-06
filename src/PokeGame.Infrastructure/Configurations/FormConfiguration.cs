using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Forms;
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
    builder.HasIndex(x => new { x.WorldId, x.VarietyId });
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Summary });
    builder.HasIndex(x => new { x.WorldId, x.PrimaryType });
    builder.HasIndex(x => new { x.WorldId, x.SecondaryType });
    builder.HasIndex(x => new { x.WorldId, x.YieldExperience });
    builder.HasIndex(x => new { x.WorldId, x.Height });
    builder.HasIndex(x => new { x.WorldId, x.Weight });
    builder.HasIndex(x => new { x.WorldId, x });

    builder.Property(x => x.Category).HasMaxLength(16).HasConversion(new EnumToStringConverter<FormCategory>());
    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);
    builder.Property(x => x.PrimaryType).HasMaxLength(8).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.SecondaryType).HasMaxLength(8).HasConversion(new EnumToStringConverter<PokemonType>());

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variety).WithMany(x => x.Forms).OnDelete(DeleteBehavior.Restrict);
  }
}
