using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Api.Models.Form;

public record SearchFormsParameters : SearchParameters
{
  [FromQuery(Name = "variety")]
  public string? Variety { get; set; }

  [FromQuery(Name = "category")]
  public FormCategory? Category { get; set; }

  [FromQuery(Name = "type")]
  public PokemonType? Type { get; set; }

  [FromQuery(Name = "ability")]
  public string? Ability { get; set; }

  public virtual SearchFormsPayload ToPayload()
  {
    SearchFormsPayload payload = new();
    payload.Variety = Variety;
    payload.Category = Category;
    payload.Type = Type;
    payload.Ability = Ability;
    Fill(payload);
    return payload;
  }
}
