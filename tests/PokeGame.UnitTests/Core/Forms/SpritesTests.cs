namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class SpritesTests
{
  [Fact(DisplayName = "It should construct Sprites from valid arguments.")]
  public void Given_ValidArguments_When_ctor_Then_Sprites()
  {
    Url @default = new("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png");
    Url shiny = new("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png");
    Url alternative = new("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png");
    Url alternativeShiny = new("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png");
    Sprites sprites = new(@default, shiny, alternative, alternativeShiny);
    Assert.Equal(@default, sprites.Default);
    Assert.Equal(shiny, sprites.Shiny);
    Assert.Equal(alternative, sprites.Alternative);
    Assert.Equal(alternativeShiny, sprites.AlternativeShiny);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    Url url = new("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png");
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Sprites(url, url, url, url));
    Assert.Equal(12, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Default");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Shiny");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Alternative");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "AlternativeShiny");
  }
}
