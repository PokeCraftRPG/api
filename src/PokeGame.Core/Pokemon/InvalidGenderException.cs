using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

public class InvalidGenderException : DomainException
{
  public int? GenderRatio
  {
    get => (int?)Data[nameof(GenderRatio)];
    private set => Data[nameof(GenderRatio)] = value;
  }
  public PokemonGender? Gender
  {
    get => (PokemonGender?)Data[nameof(Gender)];
    private set => Data[nameof(Gender)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), GetReason(GenderRatio));
      error.Data[nameof(GenderRatio)] = GenderRatio;
      error.Data[nameof(Gender)] = Gender;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidGenderException(GenderRatio? genderRatio, PokemonGender? gender, string propertyName)
    : base(BuildMessage(genderRatio, gender, propertyName))
  {
    GenderRatio = genderRatio?.Value;
    Gender = gender;
    PropertyName = propertyName;
  }

  private static string BuildMessage(GenderRatio? genderRatio, PokemonGender? gender, string propertyName) => new ErrorMessageBuilder(GetReason(genderRatio?.Value))
    .AddData(nameof(GenderRatio), genderRatio, "<null>")
    .AddData(nameof(Gender), gender, "<null>")
    .AddData(nameof(PropertyName), propertyName)
    .Build();

  private static string GetReason(int? genderRatio) => genderRatio switch
  {
    null => "The Pokémon should not have a gender (Unknown).",
    Varieties.GenderRatio.MinimumValue => $"The Pokémon gender should be '{PokemonGender.Female}'.",
    Varieties.GenderRatio.MaximumValue => $"The Pokémon gender should be '{PokemonGender.Male}'.",
    _ => "The Pokémon should have a gender.",
  };

  public static void ThrowIfNotValid(GenderRatio? genderRatio, PokemonGender? gender, string propertyName)
  {
    if (gender.HasValue && !Enum.IsDefined(gender.Value))
    {
      throw new ArgumentOutOfRangeException(nameof(gender));
    }

    if (genderRatio is null)
    {
      if (gender.HasValue)
      {
        throw new InvalidGenderException(genderRatio, gender, propertyName);
      }
    }
    else
    {
      if (!gender.HasValue
        || (genderRatio.Equals(Varieties.GenderRatio.AllFemale) && gender.Value != PokemonGender.Female)
        || (genderRatio.Equals(Varieties.GenderRatio.AllMale) && gender.Value != PokemonGender.Male))
      {
        throw new InvalidGenderException(genderRatio, gender, propertyName);
      }
    }
  }
}
