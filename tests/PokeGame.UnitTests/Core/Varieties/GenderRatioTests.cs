using Bogus;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Varieties;

[Trait(Traits.Category, Categories.Unit)]
public class GenderRatioTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "AllFemale: it should return a gender ratio with the correct value.")]
  public void Given_GenderRatio_When_AllFemale_Then_CorrectValue()
  {
    Assert.Equal(GenderRatio.MinimumValue, GenderRatio.AllFemale.Value);
  }

  [Fact(DisplayName = "AllMale: it should return a gender ratio with the correct value.")]
  public void Given_GenderRatio_When_AllMale_Then_CorrectValue()
  {
    Assert.Equal(GenderRatio.MaximumValue, GenderRatio.AllMale.Value);
  }

  [Fact(DisplayName = "ctor: it should create a new GenderRatio.")]
  public void Given_ValidValue_When_ctor_Then_GenderRatio()
  {
    int value = _faker.Random.Int(GenderRatio.MinimumValue, GenderRatio.MaximumValue);
    GenderRatio genderRatio = new(value);
    Assert.Equal(value, genderRatio.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  [InlineData(-1)]
  [InlineData(9)]
  public void Given_Invalid_When_ctor_Then_ValidationException(int value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new GenderRatio(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "IsValid: it should return false when the Pokémon gender is not valid.")]
  public void Given_InvalidGender_When_IsValid_Then_FalseReturned()
  {
    Assert.False(GenderRatio.AllFemale.IsValid(PokemonGender.Male));
    Assert.False(GenderRatio.AllMale.IsValid(PokemonGender.Female));
  }

  [Fact(DisplayName = "IsValid: it should return true when the Pokémon gender is valid.")]
  public void Given_ValidGender_When_IsValid_Then_TrueReturned()
  {
    Assert.True(GenderRatio.AllFemale.IsValid(PokemonGender.Female));
    Assert.True(GenderRatio.AllMale.IsValid(PokemonGender.Male));

    for (int value = GenderRatio.MinimumValue + 1; value < GenderRatio.MaximumValue; value++)
    {
      GenderRatio genderRatio = new(value);
      Assert.True(genderRatio.IsValid(PokemonGender.Female));
      Assert.True(genderRatio.IsValid(PokemonGender.Male));
    }
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_GenderRatio_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(GenderRatio.MinimumValue, GenderRatio.MaximumValue);
    GenderRatio genderRatio = new(value);
    Assert.Equal(value.ToString(), genderRatio.ToString());
  }
}
