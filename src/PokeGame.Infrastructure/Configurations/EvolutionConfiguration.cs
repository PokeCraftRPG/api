using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Pokemon;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class EvolutionConfiguration : AggregateConfiguration<EvolutionEntity>, IEntityTypeConfiguration<EvolutionEntity>
{
  private const int GenderMaximumLength = 8;
  private const int LocationMaximumLength = byte.MaxValue;
  private const int TimeOfDayMaximumLength = 8;
  private const int TriggerMaximumLength = 5;

  public override void Configure(EntityTypeBuilder<EvolutionEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Evolutions), PokemonContext.Schema);
    builder.HasKey(x => x.EvolutionId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.SourceId });
    builder.HasIndex(x => new { x.WorldId, x.TargetId });
    builder.HasIndex(x => new { x.WorldId, x.Trigger });
    builder.HasIndex(x => new { x.WorldId, x.ItemId });
    builder.HasIndex(x => new { x.WorldId, x.Level });
    builder.HasIndex(x => new { x.WorldId, x.Friendship });
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.HeldItemId });
    builder.HasIndex(x => new { x.WorldId, x.KnownMoveId });
    builder.HasIndex(x => new { x.WorldId, x.Location });
    builder.HasIndex(x => new { x.WorldId, x.TimeOfDay });

    builder.Property(x => x.Trigger).HasMaxLength(TriggerMaximumLength).HasConversion(new EnumToStringConverter<EvolutionTrigger>());
    builder.Property(x => x.Gender).HasMaxLength(GenderMaximumLength).HasConversion(new EnumToStringConverter<PokemonGender>());
    builder.Property(x => x.Location).HasMaxLength(LocationMaximumLength);
    builder.Property(x => x.TimeOfDay).HasMaxLength(TimeOfDayMaximumLength).HasConversion(new EnumToStringConverter<TimeOfDay>());

    builder.HasOne(x => x.Source).WithMany(x => x.EvolvesFrom)
      .HasPrincipalKey(x => x.FormId).HasForeignKey(x => x.SourceId)
      .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Target).WithMany(x => x.EvolvingInto)
      .HasPrincipalKey(x => x.FormId).HasForeignKey(x => x.TargetId)
      .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Item).WithMany(x => x.Evolutions)
      .HasPrincipalKey(x => x.ItemId).HasForeignKey(x => x.ItemId)
      .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.HeldItem).WithMany(x => x.HeldEvolutions)
      .HasPrincipalKey(x => x.ItemId).HasForeignKey(x => x.HeldItemId)
      .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.KnownMove).WithMany(x => x.Evolutions)
      .HasPrincipalKey(x => x.MoveId).HasForeignKey(x => x.KnownMoveId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
