using Krakenar.Contracts;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public static class UserExtensions
{
  public static MultiFactorAuthenticationMode GetMultiFactorAuthenticationMode(this User user)
  {
    CustomAttribute? customAttribute = user.GetCustomAttribute(UserHelper.MultiFactorAuthenticationModeKey);
    if (customAttribute is null)
    {
      return MultiFactorAuthenticationMode.None;
    }
    else if (Enum.TryParse(customAttribute.Value.Trim(), ignoreCase: true, out MultiFactorAuthenticationMode multiFactorAuthenticationMode) && Enum.IsDefined(multiFactorAuthenticationMode))
    {
      return multiFactorAuthenticationMode;
    }
    throw new ArgumentException($"The user does not have a valid '{UserHelper.MultiFactorAuthenticationModeKey}' custom attribute.", nameof(user));
  }

  public static string GetSubject(this User user) => user.Id.ToString();

  public static bool IsProfileCompleted(this User user)
  {
    CustomAttribute? customAttribute = user.GetCustomAttribute(UserHelper.ProfileCompletedOnKey);
    if (customAttribute is null)
    {
      return false;
    }
    else if (DateTime.TryParse(customAttribute.Value.Trim(), CultureInfo.InvariantCulture, out _))
    {
      return true;
    }
    throw new ArgumentException($"The user does not have a valid '{UserHelper.ProfileCompletedOnKey}' custom attribute.", nameof(user));
  }

  private static CustomAttribute? GetCustomAttribute(this User user, string key)
  {
    CustomAttribute[] customAttributes = user.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The user has many ({customAttributes.Length}) user attributes '{key}'.", nameof(user));
    }
    return customAttributes.SingleOrDefault();
  }
}
