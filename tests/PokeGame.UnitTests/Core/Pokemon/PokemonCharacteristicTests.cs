using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonCharacteristicTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct a characteristic from arguments.")]
  public void Given_Arguments_When_ctor_Then_Instance()
  {
    string text = "  LovesToEat  ";
    PokemonCharacteristic characteristic = new(text);
    Assert.Equal(text.Trim(), characteristic.Text);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the text is empty.")]
  [InlineData("")]
  [InlineData("    ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string text)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokemonCharacteristic(text));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Text");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the text is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string text = _faker.Random.String(PokemonCharacteristic.MaximumLength + 1, 'a', 'z');
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new PokemonCharacteristic(text));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Text");
  }

  [Fact(DisplayName = "ToString: it should return the characteristic text.")]
  public void Given_Characteristic_Then_ToString_Then_Text()
  {
    PokemonCharacteristic characteristic = new("  LovesToEat  ");
    Assert.Equal(characteristic.Text, characteristic.ToString());
  }
}
