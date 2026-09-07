namespace PokeGame.Core.Membership.Models;

public record RevokeMembershipPayload
{
  public Guid UserId { get; set; }
}
