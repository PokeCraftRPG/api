using FluentValidation;
using PokeGame.Core.Forms.Validators;

namespace PokeGame.Core.Forms;

public interface ITypes
{
  PokemonType Primary { get; }
  PokemonType? Secondary { get; }
}

public record Types : ITypes
{
  public PokemonType Primary { get; }
  public PokemonType? Secondary { get; }

  public Types()
  {
  }

  [JsonConstructor]
  public Types(PokemonType primary, PokemonType? secondary = null)
  {
    Primary = primary;
    Secondary = secondary;
    new TypesValidator().ValidateAndThrow(this);
  }

  public Types(ITypes types) : this(types.Primary, types.Secondary)
  {
  }
}
