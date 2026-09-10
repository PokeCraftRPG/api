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
  public override void Configure(EntityTypeBuilder<PokemonEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Specimens), PokemonContext.Schema);
    builder.HasKey(x => x.PokemonId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.SpeciesId });
    builder.HasIndex(x => new { x.WorldId, x.VarietyId });
    builder.HasIndex(x => new { x.WorldId, x.FormId });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Nickname });
    builder.HasIndex(x => new { x.WorldId, x.Summary });
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.IsShiny });
    builder.HasIndex(x => new { x.WorldId, x.HeldItemId });

    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Nickname).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);
    builder.Property(x => x.Gender).HasMaxLength(8).HasConversion(new EnumToStringConverter<Gender>());
    builder.Property(x => x.TeraType).HasMaxLength(8).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.AbilitySlot).HasMaxLength(16).HasConversion(new EnumToStringConverter<AbilitySlot>());
    builder.Property(x => x.Nature).HasMaxLength(PokemonNature.MaximumLength);
    builder.Property(x => x.GrowthRate).HasMaxLength(16).HasConversion(new EnumToStringConverter<GrowthRate>());
    builder.Property(x => x.Condition).HasMaxLength(16).HasConversion(new EnumToStringConverter<StatusCondition>());
    builder.Property(x => x.Characteristic).HasMaxLength(32).HasConversion(new EnumToStringConverter<PokemonCharacteristic>());

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Species).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Variety).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Form).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.HeldItem).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Sprite).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
