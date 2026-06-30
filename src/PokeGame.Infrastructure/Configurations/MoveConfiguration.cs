using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Validation;
using PokeGame.Infrastructure.Db;

namespace PokeGame.Infrastructure.Configurations;

internal class MoveConfiguration : IEntityTypeConfiguration<Move>
{
  public void Configure(EntityTypeBuilder<Move> builder)
  {
    builder.ToTable(nameof(PokemonContext.Moves), Schemas.Pokemon);
    builder.HasKey(x => x.MoveId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Type });
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Accuracy });
    builder.HasIndex(x => new { x.WorldId, x.Power });
    builder.HasIndex(x => new { x.WorldId, x.PowerPoints });
    builder.HasIndex(x => new { x.WorldId, x.Version });
    builder.HasIndex(x => new { x.WorldId, x.CreatedBy });
    builder.HasIndex(x => new { x.WorldId, x.CreatedOn });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedBy });
    builder.HasIndex(x => new { x.WorldId, x.UpdatedOn });

    builder.Property(x => x.Type).HasMaxLength(8).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.Category).HasMaxLength(8).HasConversion(new EnumToStringConverter<MoveCategory>());
    builder.Property(x => x.Key).HasMaxLength(Constants.KeyMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Moves)
      .HasForeignKey(x => x.WorldId).HasPrincipalKey(x => x.Id)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
