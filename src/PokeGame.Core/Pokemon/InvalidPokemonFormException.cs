using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonFormException : DomainException
{
  public InvalidPokemonFormException(Specimen specimen, Form form)
    : base("The specified Pokémon form does not belong to the Pokémon’s variety.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["VarietyId"] = specimen.VarietyId.EntityId;
    Data["AttemptedVarietyId"] = form.VarietyId.EntityId;
    Data["AttemptedFormId"] = form.EntityId;
    Data["PropertyName"] = nameof(Specimen.FormId);
  }
}
