using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonGenderException : DomainException
{
  public InvalidPokemonGenderException(Specimen specimen, Variety variety, Gender attemptedGender)
    : base("The specified gender is not allowed for this variety.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["VarietyId"] = variety.EntityId;
    Data["FemaleRate"] = variety.GenderRatio?.FemaleRate;
    Data["AttemptedGender"] = attemptedGender;
    Data["PropertyName"] = nameof(Specimen.Gender);
  }

  public static void ThrowIfNotValid(Specimen specimen, Variety variety, Gender attemptedGender)
  {
    GenderRatio? ratio = variety.GenderRatio;
    if (ratio is null
      || (Equals(ratio, GenderRatio.AllFemale) && attemptedGender == Gender.Male)
      || (Equals(ratio, GenderRatio.AllMale) && attemptedGender == Gender.Female))
    {
      throw new InvalidPokemonGenderException(specimen, variety, attemptedGender);
    }
  }
}
