using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Models;

public class MemberInvitationDto : Aggregate
{
  public WorldDto World { get; set; } = new();
  public Actor Invitee { get; set; } = new();

  public MemberInvitationStatus Status { get; set; }
  public DateTime? ExpiresOn { get; set; }
}
