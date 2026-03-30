namespace PokeGame.Core.Identity;

[Trait(Traits.Category, Categories.Unit)]
public class UserHelperTests
{
  [Fact(DisplayName = "NormalizeGender: it should normalize a gender.")]
  public void Given_Value_When_NormalizeGender_Then_Normalized()
  {
    Assert.Equal("female", UserHelper.NormalizeGender("  fEmAlE  "));
  }
}
