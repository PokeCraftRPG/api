using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class VarietyMoveConfiguration : IEntityTypeConfiguration<VarietyMoveEntity>
{
  public void Configure(EntityTypeBuilder<VarietyMoveEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.VarietyMoves), PokemonContext.Schema);
    builder.HasKey(x => new { x.VarietyId, x.MoveId });

    builder.HasOne(x => x.Variety).WithMany(x => x.Moves).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(x => x.Move).WithMany(x => x.Varieties).OnDelete(DeleteBehavior.Restrict);
  }
}
