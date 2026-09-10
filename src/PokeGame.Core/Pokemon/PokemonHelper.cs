using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

internal static class PokemonHelper
{
  public static AbilitySlot ResolveAbilitySlot(IPokemonRandomizer randomizer, Specimen specimen, Form form, AbilitySlot? slot)
  {
    if (slot.HasValue)
    {
      if (!Enum.IsDefined(slot.Value))
      {
        throw new ArgumentOutOfRangeException(nameof(slot));
      }

      if ((slot.Value == AbilitySlot.Secondary && !form.Abilities.SecondaryId.HasValue)
        || (slot.Value == AbilitySlot.Hidden && !form.Abilities.HiddenId.HasValue))
      {
        throw new InvalidAbilitySlotException(specimen, form, slot.Value);
      }

      return slot.Value;
    }

    return randomizer.AbilitySlot(form.Abilities);
  }

  public static Gender? ResolveGender(IPokemonRandomizer randomizer, Specimen specimen, Variety variety, Gender? gender)
  {
    GenderRatio? ratio = variety.GenderRatio;

    if (gender.HasValue)
    {
      if (!Enum.IsDefined(gender.Value))
      {
        throw new ArgumentOutOfRangeException(nameof(gender));
      }

      if (ratio is null || (Equals(ratio, GenderRatio.AllFemale) && gender == Gender.Male) || (Equals(ratio, GenderRatio.AllMale) && gender == Gender.Female))
      {
        throw new InvalidPokemonGenderException(specimen, variety, gender.Value);
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
