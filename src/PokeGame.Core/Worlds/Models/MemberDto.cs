using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Worlds.Models;

public record MemberDto
{
  public Actor User { get; set; } = new();

  public Actor GrantedBy { get; set; } = new();
  public DateTime GrantedOn { get; set; }
}
