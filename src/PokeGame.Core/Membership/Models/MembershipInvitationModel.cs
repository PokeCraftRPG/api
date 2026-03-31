using Krakenar.Contracts;
using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Membership.Models;

public class MembershipInvitationModel : Aggregate
{
  public string EmailAddress { get; set; } = string.Empty;
  public Actor? Invitee { get; set; }

  public MembershipInvitationStatus Status { get; set; }
  public DateTime? ExpiresOn { get; set; }
}
