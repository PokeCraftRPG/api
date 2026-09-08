using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core;
using PokeGame.Core.Trainers;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class TrainerConfiguration : AggregateConfiguration<TrainerEntity>, IEntityTypeConfiguration<TrainerEntity>
{
  public override void Configure(EntityTypeBuilder<TrainerEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Trainers), PokemonContext.Schema);
    builder.HasKey(x => x.TrainerId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Summary });
    builder.HasIndex(x => new { x.WorldId, x.License }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.Money });
    builder.HasIndex(x => x.MemberId);
    builder.HasIndex(x => new { x.WorldId, x.MemberId });

    builder.Property(x => x.Key).HasMaxLength(Key.MaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.Summary).HasMaxLength(Summary.MaximumLength);
    builder.Property(x => x.License).HasMaxLength(License.MaximumLength);
    builder.Property(x => x.Gender).HasMaxLength(8).HasConversion(new EnumToStringConverter<Gender>());
    builder.Property(x => x.MemberId).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Sprite).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
