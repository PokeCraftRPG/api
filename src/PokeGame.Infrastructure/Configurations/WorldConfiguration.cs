using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core.Validation;
using PokeGame.Core.Worlds;
using PokeGame.Infrastructure.Db;

namespace PokeGame.Infrastructure.Configurations;

internal class WorldConfiguration : IEntityTypeConfiguration<World>
{
  public void Configure(EntityTypeBuilder<World> builder)
  {
    builder.ToTable(nameof(PokemonContext.Worlds), Schemas.Pokemon);
    builder.HasKey(x => x.WorldId);

    builder.HasIndex(x => x.Id).IsUnique();
    builder.HasIndex(x => x.OwnerId);
    builder.HasIndex(x => x.Key).IsUnique();
    builder.HasIndex(x => x.Name);
    builder.HasIndex(x => x.Version);
    builder.HasIndex(x => x.CreatedBy);
    builder.HasIndex(x => x.CreatedOn);
    builder.HasIndex(x => x.UpdatedBy);
    builder.HasIndex(x => x.UpdatedOn);

    builder.Property(x => x.Key).HasMaxLength(Constants.KeyMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
  }
}
