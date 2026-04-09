using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Worlds.Models;

public class MembershipModel
{
  public Actor Member { get; set; } = new();

  public bool IsActive { get; set; }

  public Actor GrantedBy { get; set; } = new();
  public DateTime GrantedOn { get; set; }

  public Actor? RevokedBy { get; set; }
  public DateTime? RevokedOn { get; set; }

  public override bool Equals(object? obj) => obj is MembershipModel membrership && membrership.Member.Equals(Member);
  public override int GetHashCode() => Member.GetHashCode();
  public override string ToString() => Member.ToString();
}
