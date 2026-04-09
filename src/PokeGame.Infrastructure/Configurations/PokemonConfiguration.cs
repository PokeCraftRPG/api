using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class PokemonConfiguration : AggregateConfiguration<PokemonEntity>, IEntityTypeConfiguration<PokemonEntity>
{
  private const int GenderMaximumLength = 8;
  private const int SizeMaximumLength = 10;
  private const int StatisticsMaximumLength = 100;
  private const int StatusConditionMaximumLength = 10;

  public override void Configure(EntityTypeBuilder<PokemonEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Pokemon), PokemonContext.Schema);
    builder.HasKey(x => x.PokemonId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.SpeciesId });
    builder.HasIndex(x => new { x.WorldId, x.VarietyId });
    builder.HasIndex(x => new { x.WorldId, x.FormId });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.IsShiny });
    builder.HasIndex(x => new { x.WorldId, x.Size });
    builder.HasIndex(x => new { x.WorldId, x.IsEgg });
    builder.HasIndex(x => new { x.WorldId, x.Level });
    builder.HasIndex(x => new { x.WorldId, x.HeldItemId });

    builder.Property(x => x.Key).HasMaxLength(Constants.SlugMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.Gender).HasMaxLength(GenderMaximumLength).HasConversion(new EnumToStringConverter<PokemonGender>());
    builder.Property(x => x.TeraType).HasMaxLength(Constants.PokemonTypeMaximumLength).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.Size).HasMaxLength(SizeMaximumLength).HasConversion(new EnumToStringConverter<PokemonSizeCategory>());
    builder.Property(x => x.AbilitySlot).HasMaxLength(Constants.AbilitySlotMaximumLength).HasConversion(new EnumToStringConverter<AbilitySlot>());
    builder.Property(x => x.Nature).HasMaxLength(PokemonNature.MaximumLength);
    builder.Property(x => x.GrowthRate).HasMaxLength(Constants.GrowthRateMaximumLength).HasConversion(new EnumToStringConverter<GrowthRate>());
    builder.Property(x => x.Statistics).HasMaxLength(StatisticsMaximumLength);
    builder.Property(x => x.StatusCondition).HasMaxLength(StatusConditionMaximumLength).HasConversion(new EnumToStringConverter<StatusCondition>());
    builder.Property(x => x.Characteristic).HasMaxLength(PokemonCharacteristic.MaximumLength);
    builder.Property(x => x.Sprite).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Pokemon).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Species).WithMany(x => x.Pokemon).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variety).WithMany(x => x.Pokemon).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Form).WithMany(x => x.Pokemon).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.HeldItem).WithMany(x => x.HoldingPokemon)
      .HasForeignKey(x => x.HeldItemId).HasPrincipalKey(x => x.ItemId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
