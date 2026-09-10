using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

internal static class PokemonHelper
{
  public static AbilitySlot ResolveAbilitySlot(IPokemonRandomizer randomizer, FormAbilities abilites, AbilitySlot? slot)
  {
    if (slot.HasValue)
    {
      if (!Enum.IsDefined(slot.Value))
      {
        throw new ArgumentOutOfRangeException(nameof(slot));
      }

      if ((slot.Value == AbilitySlot.Secondary && !abilites.SecondaryId.HasValue) || (slot.Value == AbilitySlot.Hidden && !abilites.HiddenId.HasValue))
      {
        throw new NotImplementedException(); // TODO(fpion): 422
      }

      return slot.Value;
    }

    return randomizer.AbilitySlot(abilites);
  }

  public static Gender? ResolveGender(IPokemonRandomizer randomizer, GenderRatio? ratio, Gender? gender)
  {
    if (gender.HasValue)
    {
      if (!Enum.IsDefined(gender.Value))
      {
        throw new ArgumentOutOfRangeException(nameof(gender));
      }

      if (ratio is null || (Equals(ratio, GenderRatio.AllFemale) && gender == Gender.Male) || (Equals(ratio, GenderRatio.AllMale) && gender == Gender.Female))
      {
        throw new NotImplementedException(); // TODO(fpion): 422
      }

      return gender;
    }
    else if (ratio is not null)
    {
      return randomizer.Gender(ratio);
    }

    return null;
  }
}
