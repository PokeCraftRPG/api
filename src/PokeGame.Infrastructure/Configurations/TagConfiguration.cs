using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Core;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
  public void Configure(EntityTypeBuilder<TagEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.Tags), PokemonContext.Schema);
    builder.HasKey(x => x.TagId);

    builder.HasIndex(x => new { x.TrainerId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.TrainerId, x.Name });

    builder.Property(x => x.Name).HasMaxLength(Name.MaximumLength);
    builder.Property(x => x.CreatedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.UpdatedBy).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.Trainer).WithMany().OnDelete(DeleteBehavior.Cascade);
  }
}
