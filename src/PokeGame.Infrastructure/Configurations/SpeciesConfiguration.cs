using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class SpeciesConfiguration : AggregateConfiguration<SpeciesEntity>, IEntityTypeConfiguration<SpeciesEntity>
{
  private const int CategoryMaximumLength = 10;
  private const int EggGroupMaximumLength = 16

  public override void Configure(EntityTypeBuilder<SpeciesEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Species), PokemonContext.Schema);
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

    builder.Property(x => x.Category).HasMaxLength(CategoryMaximumLength).HasConversion(new EnumToStringConverter<PokemonCategory>());
    builder.Property(x => x.Key).HasMaxLength(Constants.SlugMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.GrowthRate).HasMaxLength(Constants.GrowthRateMaximumLength).HasConversion(new EnumToStringConverter<GrowthRate>());
    builder.Property(x => x.PrimaryEggGroup).HasMaxLength(EggGroupMaximumLength).HasConversion(new EnumToStringConverter<EggGroup>());
    builder.Property(x => x.SecondaryEggGroup).HasMaxLength(EggGroupMaximumLength).HasConversion(new EnumToStringConverter<EggGroup>());
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Species).OnDelete(DeleteBehavior.Restrict);
  }
}
