using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Species;
using PokeGame.Core.Validation;
using PokeGame.Infrastructure.Db;

namespace PokeGame.Infrastructure.Configurations;

internal class PokemonSpeciesConfiguration : IEntityTypeConfiguration<PokemonSpecies>
{
  public void Configure(EntityTypeBuilder<PokemonSpecies> builder)
  {
    builder.ToTable(nameof(PokemonContext.Species), Schemas.Pokemon);
    builder.HasKey(x => x.SpeciesId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Number }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.BaseFriendship });
    builder.HasIndex(x => new { x.WorldId, x.CatchRate });
    builder.HasIndex(x => new { x.WorldId, x.GrowthRate });
    builder.HasIndex(x => new { x.WorldId, x.EggCycles });
    builder.HasIndex(x => new { x.WorldId, x.PrimaryEggGroup });
    builder.HasIndex(x => new { x.WorldId, x.SecondaryEggGroup });
    builder.HasIndex(x => new { x.WorldId, x.Version });
    builder.HasIndex(x => new { x.WorldId, x.CreatedBy });
    builder.HasIndex(x => new { x.WorldId, x.CreatedOn });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedBy });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedOn });

    builder.Property(x => x.Category).HasMaxLength(16).HasConversion(new EnumToStringConverter<PokemonCategory>());
    builder.Property(x => x.Key).HasMaxLength(Constants.KeyMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.GrowthRate).HasMaxLength(16).HasConversion(new EnumToStringConverter<GrowthRate>());
    builder.Property(x => x.PrimaryEggGroup).HasMaxLength(16).HasConversion(new EnumToStringConverter<EggGroup>());
    builder.Property(x => x.SecondaryEggGroup).HasMaxLength(16).HasConversion(new EnumToStringConverter<EggGroup>());

    builder.HasOne(x => x.World).WithMany(x => x.Species)
      .HasForeignKey(x => x.WorldId).HasPrincipalKey(x => x.Id)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
