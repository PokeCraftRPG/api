using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonFormCategoryException : DomainException
{
  public InvalidPokemonFormCategoryException(Specimen specimen, Form form)
    : base("A Pokémon specimen can only by created using a default or alternative form.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["FormId"] = form.EntityId;
    Data["AttemptedCategory"] = form.Category;
    Data["PropertyName"] = nameof(Specimen.FormId);
  }
}
