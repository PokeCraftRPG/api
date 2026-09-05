using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Configurations;

internal class MemberInvitationConfiguration : IEntityTypeConfiguration<MemberInvitationEntity>
{
  public void Configure(EntityTypeBuilder<MemberInvitationEntity> builder)
  {
    builder.ToTable(nameof(PokemonContext.MemberInvitations), PokemonContext.Schema);
    builder.HasKey(x => x.MemberInvitationId);

    builder.HasIndex(x => new { x.WorldId, x.Id }).IsUnique();
    builder.HasIndex(x => x.EmailAddress);
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.Status);
    builder.HasIndex(x => x.ExpiresOn);

    builder.Property(x => x.EmailAddress).HasMaxLength(EmailAddress.MaximumLength);
    builder.Property(x => x.UserId).HasMaxLength(ActorId.MaximumLength);
    builder.Property(x => x.Status).HasMaxLength(16).HasConversion(new EnumToStringConverter<MemberInvitationStatus>());

    builder.HasOne(x => x.World).WithMany().OnDelete(DeleteBehavior.Restrict);
  }
}
