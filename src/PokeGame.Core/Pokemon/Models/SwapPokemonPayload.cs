using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record SwapPokemonPayload
{
  public List<Guid> PokemonIds { get; set; } = [];

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<SwapPokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.PokemonIds).Must(x => x.Count == 2 && x.Distinct().Count() == 2)
        .WithErrorCode("PokemonIdsValidator")
        .WithMessage("Exactly two different Pokémon must be specified.");
    }
  }
}
