using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class MembershipInvitations
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.MembershipInvitations), alias: null);

  public static readonly ColumnId CreatedBy = new(nameof(MembershipInvitationEntity.CreatedBy), Table);
  public static readonly ColumnId CreatedOn = new(nameof(MembershipInvitationEntity.CreatedOn), Table);
  public static readonly ColumnId StreamId = new(nameof(MembershipInvitationEntity.StreamId), Table);
  public static readonly ColumnId UpdatedBy = new(nameof(MembershipInvitationEntity.UpdatedBy), Table);
  public static readonly ColumnId UpdatedOn = new(nameof(MembershipInvitationEntity.UpdatedOn), Table);
  public static readonly ColumnId Version = new(nameof(MembershipInvitationEntity.Version), Table);

  public static readonly ColumnId EmailAddress = new(nameof(MembershipInvitationEntity.EmailAddress), Table);
  public static readonly ColumnId EmailAddressNormalized = new(nameof(MembershipInvitationEntity.EmailAddressNormalized), Table);
  public static readonly ColumnId ExpiresOn = new(nameof(MembershipInvitationEntity.ExpiresOn), Table);
  public static readonly ColumnId Id = new(nameof(MembershipInvitationEntity.Id), Table);
  public static readonly ColumnId InviteeId = new(nameof(MembershipInvitationEntity.InviteeId), Table);
  public static readonly ColumnId MembershipInvitationId = new(nameof(MembershipInvitationEntity.MembershipInvitationId), Table);
  public static readonly ColumnId Status = new(nameof(MembershipInvitationEntity.Status), Table);
  public static readonly ColumnId UserId = new(nameof(MembershipInvitationEntity.UserId), Table);
  public static readonly ColumnId WorldId = new(nameof(MembershipInvitationEntity.WorldId), Table);
}
