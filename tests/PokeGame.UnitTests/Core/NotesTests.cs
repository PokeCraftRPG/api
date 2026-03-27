using Bogus;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class NotesTests
{
  private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus tempor risus sapien, vitae molestie ex dictum eu. Vestibulum lobortis odio ut justo accumsan sodales. Donec tempus odio dui, eu vehicula diam eleifend at. Maecenas suscipit, elit malesuada volutpat malesuada, velit ipsum auctor sem, sit amet tincidunt augue lacus eget turpis. Quisque sed libero placerat, imperdiet nibh id, tristique orci. Quisque gravida vitae orci in hendrerit. Praesent dapibus vel sapien vitae hendrerit. Sed fermentum dapibus semper. Integer a fermentum est. Praesent quis efficitur nibh. Donec sollicitudin, lacus vel dignissim tincidunt, dolor ex tincidunt dolor, sed lobortis velit ex nec elit. Ut finibus varius laoreet. Nunc rutrum tristique tortor et laoreet. Phasellus faucibus luctus est, ullamcorper mollis mi ultrices sit amet.";

  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Notes.")]
  public void Given_ValidValue_When_ctor_Then_Notes()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    Notes notes = new(value);
    Assert.Equal(value.Trim(), notes.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is empty.")]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Notes(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Notes_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Notes(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Notes_When_ToString_Then_CorrectValue()
  {
    Notes notes = new(LoremIpsum);
    Assert.Equal(notes.Value, notes.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Notes when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Notes()
  {
    string value = string.Concat("  ", LoremIpsum, "  ");
    Notes? notes = Notes.TryCreate(value);
    Assert.NotNull(notes);
    Assert.Equal(value.Trim(), notes.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Notes.TryCreate(value));
  }
}
