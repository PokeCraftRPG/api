using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Membership;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MembershipInvitationConfiguration : AggregateConfiguration<MembershipInvitationEntity>, IEntityTypeConfiguration<MembershipInvitationEntity>
{
  private const int EmailAddressMaximumLength = byte.MaxValue;
  private const int StatusMaximumLength = 10;

  public override void Configure(EntityTypeBuilder<MembershipInvitationEntity> builder)
  {
    base.Configure(builder);

    builder.ToTable(nameof(PokemonContext.MembershipInvitations), PokemonContext.Schema);
    builder.HasKey(x => x.MembershipInvitationId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => new { x.WorldId, x.EmailAddressNormalized });
    builder.HasIndex(x => x.InviteeId);
    builder.HasIndex(x => new { x.WorldId, x.UserId });
    builder.HasIndex(x => new { x.WorldId, x.Status });
    builder.HasIndex(x => new { x.WorldId, x.ExpiresOn });

    builder.Property(x => x.EmailAddress).HasMaxLength(EmailAddressMaximumLength);
    builder.Property(x => x.EmailAddressNormalized).HasMaxLength(EmailAddressMaximumLength);
    builder.Property(x => x.InviteeId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.Status).HasMaxLength(StatusMaximumLength).HasConversion(new EnumToStringConverter<MembershipInvitationStatus>());

    builder.HasOne(x => x.World).WithMany(x => x.MembershipInvitations);
  }
}
