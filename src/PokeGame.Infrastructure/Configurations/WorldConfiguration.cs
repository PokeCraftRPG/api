using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class WorldConfiguration : AggregateConfiguration<WorldEntity>, IEntityTypeConfiguration<WorldEntity>
{
  public override void Configure(EntityTypeBuilder<WorldEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Worlds), PokemonContext.Schema);
    builder.HasKey(x => x.WorldId);

    builder.HasIndex(x => x.Id).IsUnique();
    builder.HasIndex(x => x.OwnerId);
    builder.HasIndex(x => x.Key).IsUnique();
    builder.HasIndex(x => x.Name);

    builder.Property(x => x.OwnerId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.Key).HasMaxLength(Slug.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
  }
}
