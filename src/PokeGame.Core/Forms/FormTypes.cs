using FluentValidation;

namespace PokeGame.Core.Forms;

public interface IFormTypes
{
  PokemonType Primary { get; }
  PokemonType? Secondary { get; }
}

public sealed record FormTypes : IFormTypes
{
  public PokemonType Primary { get; }
  public PokemonType? Secondary { get; }

  public FormTypes(PokemonType primary = PokemonType.Normal, PokemonType? secondary = null)
  {
    Primary = primary;
    Secondary = secondary;
    new FormTypesValidator().ValidateAndThrow(this);
  }

  public static FormTypes From(IFormTypes types) => new(types.Primary, types.Secondary);
}

internal class FormTypesValidator : AbstractValidator<IFormTypes>
{
  public FormTypesValidator()
  {
    RuleFor(x => x.Primary).IsInEnum();
    RuleFor(x => x.Secondary).IsInEnum().NotEqual(x => x.Primary);
  }
}
