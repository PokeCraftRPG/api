using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core;
using PokeGame.Infrastructure.Db;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MoveConfiguration : AggregateConfiguration<MoveEntity>, IEntityTypeConfiguration<MoveEntity>
{
  public override void Configure(EntityTypeBuilder<MoveEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Moves), Schemas.Pokemon);
    builder.HasKey(x => x.MoveId);

    builder.HasIndex(x => new { x.WorldId, x.EntityId }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });

    builder.Property(x => x.Key).HasMaxLength(Slug.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Moves).OnDelete(DeleteBehavior.Restrict);
  }
}
