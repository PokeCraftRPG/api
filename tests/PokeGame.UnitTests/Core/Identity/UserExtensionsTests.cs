using Bogus;
using Krakenar.Contracts;
using Krakenar.Contracts.Users;
using Logitar;
using PokeGame.Builders;

namespace PokeGame.Core.Identity;

[Trait(Traits.Category, Categories.Unit)]
public class UserExtensionsTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "GetMultiFactorAuthenticationMode: it should return None when the user does not have the custom attribute.")]
  public void Given_NoCustomAtttribute_When_GetMultiFactorAuthenticationMode_Then_None()
  {
    User user = new UserBuilder(_faker).Build();
    Assert.Equal(MultiFactorAuthenticationMode.None, user.GetMultiFactorAuthenticationMode());
  }

  [Fact(DisplayName = "GetMultiFactorAuthenticationMode: it should return the Multi-Factor Authentication Mode.")]
  public void Given_ValidCustomAtttribute_When_GetMultiFactorAuthenticationMode_Then_CorrectValue()
  {
    MultiFactorAuthenticationMode mode = _faker.PickRandom<MultiFactorAuthenticationMode>();

    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("MultiFactorAuthenticationMode", mode.ToString()));

    Assert.Equal(mode, user.GetMultiFactorAuthenticationMode());
  }

  [Fact(DisplayName = "GetMultiFactorAuthenticationMode: it should throw ArgumentException when the custom attribute is not valid.")]
  public void Given_InvalidCustomAttribute_When_GetMultiFactorAuthenticationMode_Then_ArgumentException()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("MultiFactorAuthenticationMode", "invalid"));

    var exception = Assert.Throws<ArgumentException>(() => user.GetMultiFactorAuthenticationMode());
    Assert.Equal("user", exception.ParamName);
    Assert.StartsWith("The user does not have a valid 'MultiFactorAuthenticationMode' custom attribute.", exception.Message);
  }

  [Fact(DisplayName = "GetMultiFactorAuthenticationMode: it should throw ArgumentException when the user has multiple custom attributes.")]
  public void Given_MultipleCustomAttributes_When_GetMultiFactorAuthenticationMode_Then_ArgumentException()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("MultiFactorAuthenticationMode", MultiFactorAuthenticationMode.Email.ToString()));
    user.CustomAttributes.Add(new CustomAttribute("MultiFactorAuthenticationMode", MultiFactorAuthenticationMode.Phone.ToString()));

    var exception = Assert.Throws<ArgumentException>(() => user.GetMultiFactorAuthenticationMode());
    Assert.Equal("user", exception.ParamName);
    Assert.StartsWith("The user has many (2) user attributes 'MultiFactorAuthenticationMode'.", exception.Message);
  }

  [Fact(DisplayName = "IsProfileCompleted: it should return false when the user does not have the custom attribute.")]
  public void Given_NoCustomAtttribute_When_IsProfileCompleted_Then_False()
  {
    User user = new UserBuilder(_faker).Build();
    Assert.False(user.IsProfileCompleted());
  }

  [Fact(DisplayName = "IsProfileCompleted: it should return true when the user has completed its profile.")]
  public void Given_ValidCustomAtttribute_When_IsProfileCompleted_Then_True()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));

    Assert.True(user.IsProfileCompleted());
  }

  [Fact(DisplayName = "IsProfileCompleted: it should throw ArgumentException when the custom attribute is not valid.")]
  public void Given_InvalidCustomAttribute_When_IsProfileCompleted_Then_ArgumentException()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", "invalid"));

    var exception = Assert.Throws<ArgumentException>(() => user.IsProfileCompleted());
    Assert.Equal("user", exception.ParamName);
    Assert.StartsWith("The user does not have a valid 'ProfileCompletedOn' custom attribute.", exception.Message);
  }

  [Fact(DisplayName = "IsProfileCompleted: it should throw ArgumentException when the user has multiple custom attributes.")]
  public void Given_MultipleCustomAttributes_When_IsProfileCompleted_Then_ArgumentException()
  {
    User user = new UserBuilder(_faker).Build();
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", new DateTime(2020, 1, 1).ToISOString()));
    user.CustomAttributes.Add(new CustomAttribute("ProfileCompletedOn", DateTime.Now.ToISOString()));

    var exception = Assert.Throws<ArgumentException>(() => user.IsProfileCompleted());
    Assert.Equal("user", exception.ParamName);
    Assert.StartsWith("The user has many (2) user attributes 'ProfileCompletedOn'.", exception.Message);
  }
}
