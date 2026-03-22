using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MoveConfiguration : AggregateConfiguration<MoveEntity>, IEntityTypeConfiguration<MoveEntity>
{
  private const int CategoryMaximumLength = 8;

  public override void Configure(EntityTypeBuilder<MoveEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Moves), PokemonContext.Schema);
    builder.HasKey(x => x.MoveId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => x.Type);
    builder.HasIndex(x => x.Category);
    builder.HasIndex(x => x.Name);
    builder.HasIndex(x => x.Accuracy);
    builder.HasIndex(x => x.Power);
    builder.HasIndex(x => x.PowerPoints);

    builder.Property(x => x.Type).HasMaxLength(Constants.PokemonTypeMaximumLength).HasConversion(new EnumToStringConverter<PokemonType>());
    builder.Property(x => x.Category).HasMaxLength(CategoryMaximumLength).HasConversion(new EnumToStringConverter<MoveCategory>());
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Moves).OnDelete(DeleteBehavior.Restrict);
  }
}
