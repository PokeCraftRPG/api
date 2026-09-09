using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Regions;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class EvolutionConfiguration : AggregateConfiguration<EvolutionEntity>, IEntityTypeConfiguration<EvolutionEntity>
{
  public override void Configure(EntityTypeBuilder<EvolutionEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Evolutions), PokemonContext.Schema);
    builder.HasKey(x => x.EvolutionId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.SourceId });
    builder.HasIndex(x => new { x.WorldId, x.TargetId });
    builder.HasIndex(x => new { x.WorldId, x.Trigger });
    builder.HasIndex(x => new { x.WorldId, x.Level });
    builder.HasIndex(x => new { x.WorldId, x.Friendship });
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.ItemId });
    builder.HasIndex(x => new { x.WorldId, x.MoveId });
    builder.HasIndex(x => new { x.WorldId, x.Location });
    builder.HasIndex(x => new { x.WorldId, x.TimeOfDay });

    builder.Property(x => x.Trigger).HasMaxLength(16).HasConversion(new EnumToStringConverter<EvolutionTrigger>());
    builder.Property(x => x.Gender).HasMaxLength(8).HasConversion(new EnumToStringConverter<Gender>());
    builder.Property(x => x.Location).HasMaxLength(Location.MaximumLength);
    builder.Property(x => x.TimeOfDay).HasMaxLength(8).HasConversion(new EnumToStringConverter<TimeOfDay>());

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Source).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Target).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Item).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Move).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
