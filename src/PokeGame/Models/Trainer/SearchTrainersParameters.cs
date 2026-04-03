using Krakenar.Contracts.Search;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Models.Trainer;

public record SearchTrainersParameters : SearchParameters
{
  [FromQuery(Name = "owner")]
  public string? OwnerId { get; set; }

  [FromQuery(Name = "gender")]
  public TrainerGender? Gender { get; set; }

  public virtual SearchTrainersPayload ToPayload()
  {
    SearchTrainersPayload payload = new()
    {
      OwnerId = OwnerId,
      Gender = Gender
    };
    Fill(payload);

    foreach (SortOption sort in ((SearchPayload)payload).Sort)
    {
      if (Enum.TryParse(sort.Field, out TrainerSort field))
      {
        payload.Sort.Add(new TrainerSortOption(field, sort.IsDescending));
      }
    }

    return payload;
  }
}
