using Bogus;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class DescriptionTests
{
  private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus tempor risus sapien, vitae molestie ex dictum eu. Vestibulum lobortis odio ut justo accumsan sodales. Donec tempus odio dui, eu vehicula diam eleifend at. Maecenas suscipit, elit malesuada volutpat malesuada, velit ipsum auctor sem, sit amet tincidunt augue lacus eget turpis. Quisque sed libero placerat, imperdiet nibh id, tristique orci. Quisque gravida vitae orci in hendrerit. Praesent dapibus vel sapien vitae hendrerit. Sed fermentum dapibus semper. Integer a fermentum est. Praesent quis efficitur nibh. Donec sollicitudin, lacus vel dignissim tincidunt, dolor ex tincidunt dolor, sed lobortis velit ex nec elit. Ut finibus varius laoreet. Nunc rutrum tristique tortor et laoreet. Phasellus faucibus luctus est, ullamcorper mollis mi ultrices sit amet.";

  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Description.")]
  public void Given_ValidValue_When_ctor_Then_Description()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    Description description = new(value);
    Assert.Equal(value.Trim(), description.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(Description.MaximumLength + 1);
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Description(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Description_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Description(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Description_When_ToString_Then_CorrectValue()
  {
    Description description = new(LoremIpsum);
    Assert.Equal(description.Value, description.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Description when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Description()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    Description? description = Description.TryCreate(value);
    Assert.NotNull(description);
    Assert.Equal(value.Trim(), description.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Description.TryCreate(value));
  }

  // TODO(fpion): ValidationException when the value is null, empty or white-space
}
