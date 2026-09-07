namespace PokeGame.Core.Membership.Models;

public record TransferOwnershipPayload
{
  public Guid UserId { get; set; }
}
