using FluentValidation;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class DescriptionTests
{
  [Fact(DisplayName = "Description.MaximumLength: it should be 1000.")]
  public void Given_nothing_When_MaximumLength_Then_is1000()
  {
    Assert.Equal(1000, Description.MaximumLength);
  }

  [Fact(DisplayName = "Description constructor: it should trim surrounding whitespace and store the trimmed value.")]
  public void Given_textWithSurroundingWhitespace_When_ctor_Then_ValueIsTrimmed()
  {
    Assert.Equal("hello world", new Description("  hello world  ").Value);
  }

  [Theory(DisplayName = "Description constructor: it should reject empty or whitespace-only input after trim.")]
  [InlineData("")]
  [InlineData("   \t  ")]
  public void Given_emptyOrWhitespace_When_ctor_Then_throwsValidationException(string input)
  {
    Assert.Throws<ValidationException>(() => new Description(input));
  }

  [Fact(DisplayName = "Description constructor: it should reject a value longer than MaximumLength.")]
  public void Given_textLongerThanMaximumLength_When_ctor_Then_throwsValidationException()
  {
    string tooLong = new string('x', Description.MaximumLength + 1);
    Assert.Throws<ValidationException>(() => new Description(tooLong));
  }

  [Theory(DisplayName = "Description.TryCreate: it should return null for null, empty, or whitespace-only input.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("\t")]
  public void Given_nullOrWhiteSpace_When_TryCreate_Then_returnsNull(string? input)
  {
    Assert.Null(Description.TryCreate(input));
  }

  [Fact(DisplayName = "Description.TryCreate: it should return a description with trimmed value for valid text.")]
  public void Given_nonWhiteSpaceTextWithPadding_When_TryCreate_Then_returnsTrimmedDescription()
  {
    Description? result = Description.TryCreate("  valid  ");
    Assert.NotNull(result);
    Assert.Equal("valid", result.Value);
  }

  [Fact(DisplayName = "Description.TryCreate: it should throw when the text exceeds MaximumLength.")]
  public void Given_textLongerThanMaximumLength_When_TryCreate_Then_throwsValidationException()
  {
    string tooLong = new string('y', Description.MaximumLength + 1);
    Assert.Throws<ValidationException>(() => Description.TryCreate(tooLong));
  }

  [Fact(DisplayName = "Description.ToString: it should return the same value as Value.")]
  public void Given_constructedDescription_When_ToString_Then_matchesValue()
  {
    Description description = new Description("summary");
    Assert.Equal(description.Value, description.ToString());
  }
}
