using PokeGame.Core.Search;

namespace PokeGame.Core.Trainers.Models;

public record SearchTrainersPayload : SearchPayload<TrainerSort>
{
  public Gender? Gender { get; set; }
  public Guid? MemberId { get; set; }
}
