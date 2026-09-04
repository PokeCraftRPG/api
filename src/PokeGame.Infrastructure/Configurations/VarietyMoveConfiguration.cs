using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Moves;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class VarietyMoveConfiguration : IEntityTypeConfiguration<VarietyMoveEntity>
{
  public void Configure(EntityTypeBuilder<VarietyMoveEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.VarietyMoves), PokemonContext.Schema);
    builder.HasKey(x => x.VarietyMoveId);

    builder.HasIndex(x => new { x.VarietyId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.MoveId });
    builder.HasIndex(x => new { x.LearningMethod });
    builder.HasIndex(x => new { x.Level });
    builder.HasIndex(x => new { x.CreatedBy });
    builder.HasIndex(x => new { x.CreatedOn });
    builder.HasIndex(x => new { x.UpdatedBy });
    builder.HasIndex(x => new { x.UpdatedOn });

    builder.Property(x => x.LearningMethod).HasMaxLength(16).HasConversion(new EnumToStringConverter<LearningMethod>());
    builder.Property(x => x.CreatedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.UpdatedBy).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.Variety).WithMany(x => x.Moves).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Move).WithMany(x => x.Varieties).OnDelete(DeleteBehavior.Restrict);
  }
}
