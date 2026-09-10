using FluentValidation;
using FluentValidation.Validators;

namespace PokeGame.Core.Pokemon;

public class NatureValidator<T> : IPropertyValidator<T, string>
{
  public string Name { get; } = "NatureValidator";

  public string GetDefaultMessageTemplate(string errorCode) => "'{PropertyName}' must be a valid Pokémon nature.";

  public bool IsValid(ValidationContext<T> context, string value) => PokemonNatures.Get(value) is not null;
}
