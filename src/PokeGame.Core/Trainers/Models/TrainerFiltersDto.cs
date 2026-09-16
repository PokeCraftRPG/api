using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Trainers.Models;

public record TrainerFiltersDto
{
  public List<Actor> Members { get; set; } = [];
}
