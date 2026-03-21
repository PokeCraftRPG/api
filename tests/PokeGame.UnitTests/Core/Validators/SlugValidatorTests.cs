using FluentValidation;

namespace PokeGame.Core.Validators;

[Trait(Traits.Category, Categories.Unit)]
public class SlugValidatorTests
{
  private sealed class Example;

  private static readonly SlugValidator<Example> Validator = new();
  private static readonly ValidationContext<Example> Context = new(new Example());

  [Fact(DisplayName = "Name: it should be SlugValidator.")]
  public void Given_nothing_When_Name_Then_isSlugValidator()
  {
    Assert.Equal("SlugValidator", Validator.Name);
  }

  [Fact(DisplayName = "GetDefaultMessageTemplate: it should return the message describing hyphen-separated alphanumeric words.")]
  public void Given_anyErrorCode_When_GetDefaultMessageTemplate_Then_returnsExpectedTemplate()
  {
    string template = Validator.GetDefaultMessageTemplate(string.Empty);
    Assert.Equal(
      "'{PropertyName}' must be composed of non-empty alphanumeric words separated by hyphens (-).",
      template);
  }

  [Theory(DisplayName = "IsValid: it should return true for hyphen-separated alphanumeric words.")]
  [InlineData("a")]
  [InlineData("my-cool-slug")]
  [InlineData("Gen1-Route1")]
  public void Given_validSlug_When_IsValid_Then_returnsTrue(string value)
  {
    Assert.True(Validator.IsValid(Context, value));
  }

  [Theory(DisplayName = "IsValid: it should return false when a segment is empty or contains non-alphanumeric characters.")]
  [InlineData("")]
  [InlineData("-leading")]
  [InlineData("trailing-")]
  [InlineData("double--hyphen")]
  [InlineData("under_score")]
  [InlineData("space in slug")]
  public void Given_invalidSlug_When_IsValid_Then_returnsFalse(string value)
  {
    Assert.False(Validator.IsValid(Context, value));
  }
}
