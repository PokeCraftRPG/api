using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity;

public static class OneTimePasswordExtensions
{
  private const string PurposeKey = "Purpose"; // TODO(fpion): refactor

  public static void EnsurePurpose(this OneTimePassword oneTimePassword, string purpose)
  {
    CustomAttribute? customAttribute = oneTimePassword.GetCustomAttribute(PurposeKey);
    if (customAttribute is null || customAttribute.Value != purpose)
    {
      throw new NotImplementedException(); // TODO(fpion): implement
    }
  }

  private static CustomAttribute? GetCustomAttribute(this OneTimePassword oneTimePassword, string key)
  {
    CustomAttribute[] customAttributes = oneTimePassword.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The One-Time Password (OTP) has many ({customAttributes.Length}) user attributes '{key}'.", nameof(oneTimePassword));
    }
    return customAttributes.SingleOrDefault();
  }
}
