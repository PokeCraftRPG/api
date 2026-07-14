using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity;

public static class OneTimePasswordExtensions
{
  public const string PurposeKey = "Purpose";

  public static void EnsurePurpose(this OneTimePassword oneTimePassword, string expectedPurpose)
  {
    string? purpose = oneTimePassword.GetPurpose();
    if (purpose is null || purpose != expectedPurpose)
    {
      throw new InvalidOneTimePasswordException(oneTimePassword, expectedPurpose);
    }
  }
  public static string? GetPurpose(this OneTimePassword oneTimePassword) => oneTimePassword.GetCustomAttribute(PurposeKey)?.Value;

  private static CustomAttribute? GetCustomAttribute(this OneTimePassword oneTimePassword, string key)
  {
    CustomAttribute[] customAttributes = oneTimePassword.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length < 1)
    {
      return null;
    }
    else if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The One-Time Password (OTP) 'Id={oneTimePassword.Id}' has multiple ({customAttributes.Length}) custom attributes '{key}'.", nameof(oneTimePassword));
    }
    return customAttributes.Single();
  }
}
