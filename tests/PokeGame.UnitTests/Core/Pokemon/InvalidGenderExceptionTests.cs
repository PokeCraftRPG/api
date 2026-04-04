using Bogus;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class InvalidGenderExceptionTests
{
  private const string PropertyName = "Gender";

  private readonly Faker _faker = new();

  [Theory(DisplayName = "It should not throw when the gender is valid.")]
  [InlineData(null, null)]
  [InlineData(0, PokemonGender.Female)]
  [InlineData(8, PokemonGender.Male)]
  [InlineData(4, PokemonGender.Female)]
  [InlineData(4, PokemonGender.Male)]
  public void Given_Valid_When_ThrowIfNotValid_Then_NoException(int? genderRatio, PokemonGender? gender)
  {
    InvalidGenderException.ThrowIfNotValid(genderRatio.HasValue ? new GenderRatio(genderRatio.Value) : null, gender, PropertyName);
  }

  [Fact(DisplayName = "It should throw when the gender is not defined.")]
  public void Given_GenderNotDefined_When_ThrowIfNotValid_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => InvalidGenderException.ThrowIfNotValid(genderRatio: null, (PokemonGender)(-1), PropertyName));
    Assert.Equal("gender", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw when the gender should be null.")]
  public void Given_UnknownGender_When_ThrowIfNotValid_Then_InvalidGenderException()
  {
    PokemonGender gender = _faker.PickRandom<PokemonGender>();
    var exception = Assert.Throws<InvalidGenderException>(() => InvalidGenderException.ThrowIfNotValid(genderRatio: null, gender, PropertyName));
    Assert.StartsWith("The Pokémon should not have a gender (Unknown).", exception.Message);
  }

  [Theory(DisplayName = "It should throw when the gender should be Female.")]
  [InlineData(null)]
  [InlineData(PokemonGender.Male)]
  public void Given_NotFemale_When_ThrowIfNotValid_Then_InvalidGenderException(PokemonGender? gender)
  {
    var exception = Assert.Throws<InvalidGenderException>(() => InvalidGenderException.ThrowIfNotValid(GenderRatio.AllFemale, gender, PropertyName));
    Assert.StartsWith("The Pokémon gender should be 'Female'.", exception.Message);
  }

  [Theory(DisplayName = "It should throw when the gender should be Male.")]
  [InlineData(null)]
  [InlineData(PokemonGender.Female)]
  public void Given_NotMale_When_ThrowIfNotValid_Then_InvalidGenderException(PokemonGender? gender)
  {
    var exception = Assert.Throws<InvalidGenderException>(() => InvalidGenderException.ThrowIfNotValid(GenderRatio.AllMale, gender, PropertyName));
    Assert.StartsWith("The Pokémon gender should be 'Male'.", exception.Message);
  }

  [Fact(DisplayName = "It should throw when the gender should not be null.")]
  public void Given_NullGender_When_ThrowIfNotValid_Then_InvalidGenderException()
  {
    GenderRatio genderRatio = new(4);
    var exception = Assert.Throws<InvalidGenderException>(() => InvalidGenderException.ThrowIfNotValid(genderRatio, gender: null, PropertyName));
    Assert.StartsWith("The Pokémon should have a gender.", exception.Message);
  }
}
