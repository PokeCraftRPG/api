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
    builder.HasKey(x => x.MemberId);

    builder.HasIndex(x => x.MemberKey);
    builder.HasIndex(x => new { x.WorldId, x.UserId }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.GrantedBy });
    builder.HasIndex(x => new { x.WorldId, x.GrantedOn });
    builder.HasIndex(x => new { x.WorldId, x.RevokedBy });
    builder.HasIndex(x => new { x.WorldId, x.RevokedOn });

    builder.Property(x => x.MemberKey).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.GrantedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.RevokedBy).HasMaxLength(ActorId.MaximumLength);
  }
}
