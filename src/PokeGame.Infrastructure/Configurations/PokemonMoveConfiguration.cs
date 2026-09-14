using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Moves;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class PokemonMoveConfiguration : IEntityTypeConfiguration<PokemonMoveEntity>
{
  public void Configure(EntityTypeBuilder<PokemonMoveEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.PokemonMoves), PokemonContext.Schema);
    builder.HasKey(x => new { x.PokemonId, x.MoveId });

    builder.HasIndex(x => x.MoveId);
    builder.HasIndex(x => new { x.PokemonId, x.Slot });

    builder.Property(x => x.LearningMethod).HasMaxLength(16).HasConversion(new EnumToStringConverter<LearningMethod>());
    builder.Property(x => x.CreatedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.UpdatedBy).HasMaxLength(ActorId.MaximumLength);
  }
}
