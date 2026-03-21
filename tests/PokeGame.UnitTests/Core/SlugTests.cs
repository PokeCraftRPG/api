using FluentValidation;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class SlugTests
{
  [Fact(DisplayName = "MaximumLength: it should be 100.")]
  public void Given_nothing_When_MaximumLength_Then_is100()
  {
    Assert.Equal(100, Slug.MaximumLength);
  }

  [Fact(DisplayName = "ctor: it should trim surrounding whitespace and store the trimmed slug.")]
  public void Given_slugWithSurroundingWhitespace_When_ctor_Then_ValueIsTrimmed()
  {
    Assert.Equal("my-slug", new Slug("  my-slug  ").Value);
  }

  [Theory(DisplayName = "ctor: it should throw when the value is empty or whitespace-only after trim.")]
  [InlineData("")]
  [InlineData("   \t  ")]
  public void Given_emptyOrWhitespace_When_ctor_Then_throwsValidationException(string input)
  {
    Assert.Throws<ValidationException>(() => new Slug(input));
  }

  [Fact(DisplayName = "ctor: it should throw when the value is longer than MaximumLength.")]
  public void Given_textLongerThanMaximumLength_When_ctor_Then_throwsValidationException()
  {
    string tooLong = new string('a', Slug.MaximumLength + 1);
    Assert.Throws<ValidationException>(() => new Slug(tooLong));
  }

  [Theory(DisplayName = "ctor: it should accept hyphen-separated alphanumeric segments.")]
  [InlineData("a")]
  [InlineData("my-cool-slug")]
  [InlineData("Gen1-Route1")]
  public void Given_validSlugPattern_When_ctor_Then_succeeds(string value)
  {
    Slug slug = new Slug(value);
    Assert.Equal(value, slug.Value);
  }

  [Theory(DisplayName = "ctor: it should throw when the pattern is invalid (leading/trailing hyphen, double hyphen, underscore, or spaces).")]
  [InlineData("-leading")]
  [InlineData("trailing-")]
  [InlineData("double--hyphen")]
  [InlineData("under_score")]
  [InlineData("space in slug")]
  public void Given_invalidSlugPattern_When_ctor_Then_throwsValidationException(string value)
  {
    Assert.Throws<ValidationException>(() => new Slug(value));
  }

  [Fact(DisplayName = "ToString: it should return the same text as Value.")]
  public void Given_constructedSlug_When_ToString_Then_matchesValue()
  {
    Slug slug = new Slug("my-slug");
    Assert.Equal(slug.Value, slug.ToString());
  }
}
