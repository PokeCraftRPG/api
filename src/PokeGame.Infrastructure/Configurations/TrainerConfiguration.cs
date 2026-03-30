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
  private const int GenderMaximumLength = 8;

  public override void Configure(EntityTypeBuilder<TrainerEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.Trainers), PokemonContext.Schema);
    builder.HasKey(x => x.TrainerId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.UserId });
    builder.HasIndex(x => new { x.WorldId, x.License }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Key }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.Name });
    builder.HasIndex(x => new { x.WorldId, x.Gender });
    builder.HasIndex(x => new { x.WorldId, x.Money });
    builder.HasIndex(x => new { x.WorldId, x.PartySize });

    builder.Property(x => x.OwnerId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.License).HasMaxLength(License.MaximumLength);
    builder.Property(x => x.Key).HasMaxLength(Constants.SlugMaximumLength);
    builder.Property(x => x.Name).HasMaxLength(Constants.NameMaximumLength);
    builder.Property(x => x.Gender).HasMaxLength(GenderMaximumLength).HasConversion(new EnumToStringConverter<TrainerGender>());
    builder.Property(x => x.Sprite).HasMaxLength(Url.MaximumLength);
    builder.Property(x => x.Url).HasMaxLength(Url.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Trainers).OnDelete(DeleteBehavior.Restrict);
  }
}
