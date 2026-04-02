using Krakenar.Contracts.Search;

namespace PokeGame.Core.Trainers.Models;

public record SearchTrainersPayload : SearchPayload
{
  public string? OwnerId { get; set; }
  public TrainerGender? Gender { get; set; }

  public new List<TrainerSortOption> Sort { get; set; } = [];
}
