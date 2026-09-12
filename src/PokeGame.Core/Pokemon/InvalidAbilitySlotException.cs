using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidAbilitySlotException : DomainException
{
  public InvalidAbilitySlotException(Specimen specimen, Form form, AbilitySlot attemptedSlot)
    : base("The specified ability slot is not available on this form.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["FormId"] = form.EntityId;
    Data["AttemptedSlot"] = attemptedSlot;
    Data["PropertyName"] = nameof(Specimen.AbilitySlot);
  }
}
