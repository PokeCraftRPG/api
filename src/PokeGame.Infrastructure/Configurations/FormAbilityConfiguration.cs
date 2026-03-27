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
    builder.HasKey(x => new { x.FormId, x.AbilityId });

    builder.HasIndex(x => new { x.FormId, x.Slot }).IsUnique();

    builder.Property(x => x.Slot).HasMaxLength(Constants.AbilitySlotMaximumLength).HasConversion(new EnumToStringConverter<AbilitySlot>());

    builder.HasOne(x => x.Form).WithMany(x => x.Abilities).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Ability).WithMany(x => x.Forms).OnDelete(DeleteBehavior.Cascade);
  }
}
