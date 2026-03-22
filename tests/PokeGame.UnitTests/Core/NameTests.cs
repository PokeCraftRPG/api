using Bogus;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class NameTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Name.")]
  public void Given_ValidValue_When_ctor_Then_Name()
  {
    string value = string.Concat("  ", _faker.Company.CompanyName(), "  ");
    Name name = new(value);
    Assert.Equal(value.Trim(), name.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(Name.MaximumLength + 1);
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Name(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Name_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", _faker.Company.CompanyName(), "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Name(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Name_When_ToString_Then_CorrectValue()
  {
    Name name = new(_faker.Company.CompanyName());
    Assert.Equal(name.Value, name.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Name when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Name()
  {
    string value = string.Concat("  ", _faker.Company.CompanyName(), "  ");
    Name? name = Name.TryCreate(value);
    Assert.NotNull(name);
    Assert.Equal(value.Trim(), name.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Name.TryCreate(value));
  }
}
