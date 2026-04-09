using Logitar.Data;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.PokemonDb;

internal static class Membership
{
  public static readonly TableId Table = new(PokemonContext.Schema, nameof(PokemonContext.Membership), alias: null);

  public static readonly ColumnId GrantedBy = new(nameof(MembershipEntity.GrantedBy), Table);
  public static readonly ColumnId GrantedOn = new(nameof(MembershipEntity.GrantedOn), Table);
  public static readonly ColumnId IsActive = new(nameof(MembershipEntity.IsActive), Table);
  public static readonly ColumnId MemberId = new(nameof(MembershipEntity.MemberId), Table);
  public static readonly ColumnId RevokedBy = new(nameof(MembershipEntity.RevokedBy), Table);
  public static readonly ColumnId RevokedOn = new(nameof(MembershipEntity.RevokedOn), Table);
  public static readonly ColumnId UserId = new(nameof(MembershipEntity.UserId), Table);
  public static readonly ColumnId WorldId = new(nameof(MembershipEntity.WorldId), Table);
}
