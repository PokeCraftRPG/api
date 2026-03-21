using Krakenar.Contracts;

namespace PokeGame.Core.Abilities.Models;

public class AbilityModel : Aggregate
{
  // TODO(fpion): Key/Slug
  public string? Name { get; set; }
  public string? Description { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  // TODO(fpion): ToString
}
