using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity;

[Trait(Traits.Category, Categories.Unit)]
public class OneTimePasswordExtensionsTests
{
  private const string PurposeValue = "MultiFactorAuthentication";

  [Fact(DisplayName = "EnsurePurpose: it should not throw when the purpose is expected.")]
  public void Given_ExpectedCustomAttribute_When_EnsurePurpose_Then_DoesNotThrow()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", PurposeValue));

    oneTimePassword.EnsurePurpose(PurposeValue);
  }

  [Fact(DisplayName = "EnsurePurpose: it should throw ArgumentException when there are multiple custom attributes.")]
  public void Given_MultipleCustomAttributes_When_EnsurePurpose_Then_ArgumentException()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", "valid"));
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", "invalid"));

    var exception = Assert.Throws<ArgumentException>(() => oneTimePassword.EnsurePurpose("Purpose"));
    Assert.Equal("oneTimePassword", exception.ParamName);
    Assert.StartsWith("The One-Time Password (OTP) has many (2) user attributes 'Purpose'.", exception.Message);
  }

  [Fact(DisplayName = "EnsurePurpose: it should throw InvalidOneTimePasswordException when the purpose is not valid.")]
  public void Given_InvalidPurpose_When_EnsurePurpose_Then_InvalidOneTimePasswordException()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.Id = Guid.NewGuid();
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", PurposeValue));

    string attemptedPurpose = "PhoneVerification";
    var exception = Assert.Throws<InvalidOneTimePasswordException>(() => oneTimePassword.EnsurePurpose(attemptedPurpose));
    Assert.Equal(oneTimePassword.Id, exception.OneTimePasswordId);
    Assert.Equal(PurposeValue, exception.ExpectedPurpose);
    Assert.Equal(attemptedPurpose, exception.AttemptedPurpose);
  }

  [Fact(DisplayName = "EnsurePurpose: it should throw InvalidOneTimePasswordException when there is no purpose.")]
  public void Given_NoPurpose_When_EnsurePurpose_Then_InvalidOneTimePasswordException()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.Id = Guid.NewGuid();

    var exception = Assert.Throws<InvalidOneTimePasswordException>(() => oneTimePassword.EnsurePurpose(PurposeValue));
    Assert.Equal(oneTimePassword.Id, exception.OneTimePasswordId);
    Assert.Null(exception.ExpectedPurpose);
    Assert.Equal(PurposeValue, exception.AttemptedPurpose);
  }

  [Fact(DisplayName = "GetPurpose: it should return null when there is no custom attribute.")]
  public void Given_NoCustomAttribute_When_GetPurpose_Then_NullReturned()
  {
    OneTimePassword oneTimePassword = new();
    Assert.Null(oneTimePassword.GetPurpose());
  }

  [Fact(DisplayName = "GetPurpose: it should return the purpose found.")]
  public void Given_CustomAttribute_When_GetPurpose_Then_ValueReturned()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", PurposeValue));

    Assert.Equal(PurposeValue, oneTimePassword.GetPurpose());
  }

  [Fact(DisplayName = "GetPurpose: it should throw ArgumentException when there are multiple custom attributes.")]
  public void Given_MultipleCustomAttributes_When_GetPurpose_Then_ArgumentException()
  {
    OneTimePassword oneTimePassword = new();
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", "valid"));
    oneTimePassword.CustomAttributes.Add(new CustomAttribute("Purpose", "invalid"));

    var exception = Assert.Throws<ArgumentException>(() => oneTimePassword.GetPurpose());
    Assert.Equal("oneTimePassword", exception.ParamName);
    Assert.StartsWith("The One-Time Password (OTP) has many (2) user attributes 'Purpose'.", exception.Message);
  }
}
