using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MembershipConfiguration : IEntityTypeConfiguration<MembershipEntity>
{
  public void Configure(EntityTypeBuilder<MembershipEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.Membership), PokemonContext.Schema);
    builder.HasKey(x => new { x.WorldId, x.UserId });

    builder.HasIndex(x => x.MemberId);
    builder.HasIndex(x => new { x.WorldId, x.IsActive });
    builder.HasIndex(x => new { x.WorldId, x.GrantedBy });
    builder.HasIndex(x => new { x.WorldId, x.GrantedOn });
    builder.HasIndex(x => new { x.WorldId, x.RevokedBy });
    builder.HasIndex(x => new { x.WorldId, x.RevokedOn });

    builder.Property(x => x.MemberId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.GrantedBy).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.RevokedBy).HasMaxLength(ActorId.MaximumLength);

    builder.HasOne(x => x.World).WithMany(x => x.Membership).OnDelete(DeleteBehavior.Cascade);
  }
}
