using PokeGame.Api.Models.Search;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Api.Models.Trainer;

public record SearchTrainersParameters : SearchParameters
{
  public virtual SearchTrainersPayload ToPayload()
  {
    SearchTrainersPayload payload = new();
    Fill(payload);
    return payload;
  }
}
