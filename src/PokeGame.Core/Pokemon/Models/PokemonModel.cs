using Krakenar.Contracts;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Pokemon.Models;

public class PokemonModel : Aggregate
{
  public FormModel Form { get; set; } = new();

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public PokemonGender? Gender { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }
}
