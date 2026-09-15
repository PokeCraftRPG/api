using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Rosters.Models;

public class TagDto
{
  public Guid Id { get; set; }

  public string Name { get; set; } = string.Empty;
  public ColorDto? Color { get; set; }

  public Actor CreatedBy { get; set; } = new();
  public DateTime CreatedOn { get; set; }

  public Actor UpdatedBy { get; set; } = new();
  public DateTime UpdatedOn { get; set; }
}
