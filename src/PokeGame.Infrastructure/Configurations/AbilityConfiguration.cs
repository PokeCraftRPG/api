using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Validation;
using PokeGame.Infrastructure.Db;

namespace PokeGame.Infrastructure.Configurations;

internal class AbilityConfiguration : IEntityTypeConfiguration<Ability>
{
  public void Configure(EntityTypeBuilder<Ability> builder)
  {
    builder.ToTable(nameof(PokemonContext.Abilities), Schemas.Pokemon);
    builder.HasKey(x => x.AbilityId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Version });
    builder.HasIndex(x => new { x.WorldId, x.CreatedBy });
    builder.HasIndex(x => new { x.WorldId, x.CreatedOn });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedBy });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedOn });

    builder.Property(x => x.Key).HasMaxLength(Constants.KeyMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Abilities)
      .HasForeignKey(x => x.WorldId).HasPrincipalKey(x => x.Id)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
