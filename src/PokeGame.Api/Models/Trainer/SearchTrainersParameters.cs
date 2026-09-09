using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Api.Models.Trainer;

public record SearchTrainersParameters : SearchParameters
{
  [FromQuery(Name = "gender")]
  public Gender? Gender { get; set; }

  [FromQuery(Name = "member")]
  public Guid? MemberId { get; set; }

  public virtual SearchTrainersPayload ToPayload()
  {
    SearchTrainersPayload payload = new();
    payload.Gender = Gender;
    payload.MemberId = MemberId;
    Fill(payload);
    return payload;
  }
}
