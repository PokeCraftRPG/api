using Microsoft.AspNetCore.Mvc;
using PokeGame.Api.Models.Search;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Api.Models.Move;

public record SearchMovesParameters : SearchParameters
{
  [FromQuery(Name = "type")]
  public PokemonType? Type { get; set; }

  [FromQuery(Name = "category")]
  public MoveCategory? Category { get; set; }

  public virtual SearchMovesPayload ToPayload()
  {
    SearchMovesPayload payload = new();
    payload.Type = Type;
    payload.Category = Category;
    Fill(payload);
    return payload;
  }
}
