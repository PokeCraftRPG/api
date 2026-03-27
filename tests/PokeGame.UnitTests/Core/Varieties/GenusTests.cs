using Bogus;

namespace PokeGame.Core.Varieties;

[Trait(Traits.Category, Categories.Unit)]
public class GenusTests
{
  private const string GenusValue = "Seed";

  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Genus.")]
  public void Given_ValidValue_When_ctor_Then_Genus()
  {
    string value = string.Concat("  ", GenusValue, "  ");
    Genus genus = new(value);
    Assert.Equal(value.Trim(), genus.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is empty.")]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Genus(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(Genus.MaximumLength + 1);
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Genus(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Genus_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", GenusValue, "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Genus(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Genus_When_ToString_Then_CorrectValue()
  {
    Genus genus = new(GenusValue);
    Assert.Equal(genus.Value, genus.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Genus when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Genus()
  {
    string value = string.Concat("  ", GenusValue, "  ");
    Genus? genus = Genus.TryCreate(value);
    Assert.NotNull(genus);
    Assert.Equal(value.Trim(), genus.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Genus.TryCreate(value));
  }
}
