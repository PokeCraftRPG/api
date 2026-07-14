namespace PokeGame.Core.Identity;

public static class UserHelper
{
  public const string MultiFactorAuthenticationModeKey = "MultiFactorAuthenticationMode";
  public const string ProfileCompletedOnKey = "ProfileCompletedOn";

  public static string NormalizeGender(string gender) => gender.Trim().ToLowerInvariant();
}
