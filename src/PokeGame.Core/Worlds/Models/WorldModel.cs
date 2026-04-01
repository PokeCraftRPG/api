using Krakenar.Contracts;
using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Worlds.Models;

public class WorldModel : Aggregate
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public Actor Owner { get; set; } = new();
  public List<MembershipModel> Membership { get; set; } = [];

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
