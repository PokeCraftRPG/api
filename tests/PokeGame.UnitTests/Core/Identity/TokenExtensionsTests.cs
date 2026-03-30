using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;

namespace PokeGame.Core.Identity;

[Trait(Traits.Category, Categories.Unit)]
public class TokenExtensionsTests
{
  [Fact(DisplayName = "GetPhone: it should return null when the number is missing.")]
  public void Given_MissingNumber_When_GetPhone_Then_NullReturned()
  {
    ValidatedToken validatedToken = new();
    validatedToken.Claims.Add(new Claim(Rfc7519ClaimNames.IsPhoneVerified, bool.TrueString));
    validatedToken.Claims.Add(new Claim(CustomClaimNames.PhoneCountryCode, "CA"));
    validatedToken.Claims.Add(new Claim(CustomClaimNames.PhoneExtension, "123456"));

    Assert.Null(validatedToken.GetPhone());
  }

  [Theory(DisplayName = "GetPhone: it should return the phone from claims.")]
  [InlineData("+15145551234")]
  [InlineData("+15145551234", "CA", "123456", true)]
  public void Given_PhoneNumber_When_GetPhone_Then_PhoneReturned(string number, string? countryCode = null, string? extension = null, bool isVerified = false)
  {
    ValidatedToken validatedToken = new();
    validatedToken.Claims.Add(new Claim(Rfc7519ClaimNames.PhoneNumber, number));
    validatedToken.Claims.Add(new Claim(Rfc7519ClaimNames.IsPhoneVerified, isVerified.ToString()));
    if (countryCode is not null)
    {
      validatedToken.Claims.Add(new Claim(CustomClaimNames.PhoneCountryCode, countryCode));
    }
    if (extension is not null)
    {
      validatedToken.Claims.Add(new Claim(CustomClaimNames.PhoneExtension, extension));
    }

    PhonePayload? phone = validatedToken.GetPhone();
    Assert.NotNull(phone);
    Assert.Equal(number, phone.Number);
    Assert.Equal(countryCode, phone.CountryCode);
    Assert.Equal(extension, phone.Extension);
    Assert.Equal(isVerified, phone.IsVerified);
  }
}
