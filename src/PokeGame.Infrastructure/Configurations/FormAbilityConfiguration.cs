using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Abilities;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class FormAbilityConfiguration : IEntityTypeConfiguration<FormAbilityEntity>
{
  public void Configure(EntityTypeBuilder<FormAbilityEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.FormAbilities), PokemonContext.Schema);
    builder.HasKey(x => new { x.FormId, x.Slot });

    builder.HasIndex(x => new { x.FormId, x.Ability }).IsUnique();

    builder.Property(x => x.Slot).HasMaxLength(16).HasConversion(new EnumToStringConverter<AbilitySlot>());

    builder.HasOne(x => x.Form).WithMany(x => x.Abilities).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Ability).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
