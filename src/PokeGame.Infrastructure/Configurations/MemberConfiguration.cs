using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MemberConfiguration : IEntityTypeConfiguration<MemberEntity>
{
  public void Configure(EntityTypeBuilder<MemberEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.Members), PokemonContext.Schema);
    builder.HasKey(x => new { x.WorldId, x.UserId });

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.GrantedBy);
    builder.HasIndex(x => x.GrantedOn);

    builder.Property(x => x.UserId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.GrantedBy).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Members).OnDelete(DeleteBehavior.Cascade);
  }
}
