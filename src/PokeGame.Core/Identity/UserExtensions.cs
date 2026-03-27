using Krakenar.Contracts;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

internal static class UserExtensions
{
  private const string MultiFactorAuthenticationModeKey = "MultiFactorAuthenticationMode";
  private const string ProfileCompletedOnKey = "ProfileCompletedOn";

  public static MultiFactorAuthenticationMode GetMultiFactorAuthenticationMode(this User user)
  {
    CustomAttribute? customAttribute = user.TryGetCustomAttribute(MultiFactorAuthenticationModeKey);
    if (customAttribute is null)
    {
      return MultiFactorAuthenticationMode.None;
    }
    else if (Enum.TryParse(customAttribute.Value.Trim(), ignoreCase: true, out MultiFactorAuthenticationMode multiFactorAuthenticationMode) && Enum.IsDefined(multiFactorAuthenticationMode))
    {
      return multiFactorAuthenticationMode;
    }
    throw new ArgumentException($"The user does not have a valid '{MultiFactorAuthenticationModeKey}' custom attribute.", nameof(user));
  }

  public static bool IsProfileCompleted(this User user)
  {
    CustomAttribute? customAttribute = user.TryGetCustomAttribute(ProfileCompletedOnKey);
    if (customAttribute is null)
    {
      return false;
    }
    else if (DateTime.TryParse(customAttribute.Value.Trim(), CultureInfo.InvariantCulture, out _))
    {
      return true;
    }
    throw new ArgumentException($"The user does not have a valid '{ProfileCompletedOnKey}' custom attribute.", nameof(user));
  }

  private static CustomAttribute FindCustomAttribute(this User user, string key)
  {
    return user.TryGetCustomAttribute(key) ?? throw new ArgumentException($"The user '{user}' does not have a custom attribute '{key}'.", nameof(user));
  }
  private static CustomAttribute? TryGetCustomAttribute(this User user, string key)
  {
    CustomAttribute[] customAttributes = user.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The user has many ({customAttributes.Length}) user attributes '{key}'.", nameof(user));
    }
    return customAttributes.SingleOrDefault();
  }
}
