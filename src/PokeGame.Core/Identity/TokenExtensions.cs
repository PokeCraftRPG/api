using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;

namespace PokeGame.Core.Identity;

internal static class TokenExtensions
{
  public static PhonePayload? GetPhone(this ValidatedToken validatedToken)
  {
    string? countryCode = null;
    string? number = null;
    string? extension = null;
    bool isVerified = false;

    foreach (Claim claim in validatedToken.Claims)
    {
      switch (claim.Name)
      {
        case Rfc7519ClaimNames.PhoneNumber:
          number = claim.Value;
          break;
        case Rfc7519ClaimNames.IsPhoneVerified:
          isVerified = bool.Parse(claim.Value);
          break;
        case CustomClaimNames.PhoneCountryCode:
          countryCode = claim.Value;
          break;
        case CustomClaimNames.PhoneExtension:
          extension = claim.Value;
          break;
      }
    }

    return number is null ? null : new PhonePayload(number, countryCode, extension, isVerified);
  }
}
