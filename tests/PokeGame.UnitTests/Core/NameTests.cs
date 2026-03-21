using FluentValidation;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class NameTests
{
  [Fact(DisplayName = "Name.MaximumLength: it should be 100.")]
  public void Given_nothing_When_MaximumLength_Then_is100()
  {
    Assert.Equal(100, Name.MaximumLength);
  }

  [Fact(DisplayName = "Name constructor: it should trim surrounding whitespace and store the trimmed value.")]
  public void Given_textWithSurroundingWhitespace_When_ctor_Then_ValueIsTrimmed()
  {
    Assert.Equal("hello world", new Name("  hello world  ").Value);
  }

  [Theory(DisplayName = "Name constructor: it should reject empty or whitespace-only input after trim.")]
  [InlineData("")]
  [InlineData("   \t  ")]
  public void Given_emptyOrWhitespace_When_ctor_Then_throwsValidationException(string input)
  {
    Assert.Throws<ValidationException>(() => new Name(input));
  }

  [Fact(DisplayName = "Name constructor: it should reject a value longer than MaximumLength.")]
  public void Given_textLongerThanMaximumLength_When_ctor_Then_throwsValidationException()
  {
    string tooLong = new string('x', Name.MaximumLength + 1);
    Assert.Throws<ValidationException>(() => new Name(tooLong));
  }

  [Theory(DisplayName = "Name.TryCreate: it should return null for null, empty, or whitespace-only input.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("\t")]
  public void Given_nullOrWhiteSpace_When_TryCreate_Then_returnsNull(string? input)
  {
    Assert.Null(Name.TryCreate(input));
  }

  [Fact(DisplayName = "Name.TryCreate: it should return a name with trimmed value for valid text.")]
  public void Given_nonWhiteSpaceTextWithPadding_When_TryCreate_Then_returnsTrimmedName()
  {
    Name? result = Name.TryCreate("  valid  ");
    Assert.NotNull(result);
    Assert.Equal("valid", result.Value);
  }

  [Fact(DisplayName = "Name.TryCreate: it should throw when the text exceeds MaximumLength.")]
  public void Given_textLongerThanMaximumLength_When_TryCreate_Then_throwsValidationException()
  {
    string tooLong = new string('y', Name.MaximumLength + 1);
    Assert.Throws<ValidationException>(() => Name.TryCreate(tooLong));
  }

  [Fact(DisplayName = "Name.ToString: it should return the same value as Value.")]
  public void Given_constructedName_When_ToString_Then_matchesValue()
  {
    Name name = new Name("summary");
    Assert.Equal(name.Value, name.ToString());
  }
}
