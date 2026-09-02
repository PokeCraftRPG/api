using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MoveConfiguration : AggregateConfiguration<MoveEntity>, IEntityTypeConfiguration<MoveEntity>
{
  public override void Configure(EntityTypeBuilder<MoveEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Moves), PokemonContext.Schema);
    builder.HasKey(x => x.MoveId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Type });
    builder.HasIndex(x => new { x.WorldId, x.Category });
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Summary });
    builder.HasIndex(x => new { x.WorldId, x.Accuracy });
    builder.HasIndex(x => new { x.WorldId, x.Power });
    builder.HasIndex(x => new { x.WorldId, x.PowerPoints });

    builder.Property(x => x.Type).HasMaxLength(8).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.Category).HasMaxLength(8).HasConversion(new EnumToStringConverter<MoveCategory>());
    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
