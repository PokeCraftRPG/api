using Krakenar.Contracts;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public static class UserExtensions
{
  public static UserExperience GetDefaultExperience(this User user)
  {
    CustomAttribute? customAttribute = user.GetCustomAttribute(UserHelper.DefaultExperienceKey);
    if (customAttribute is null)
    {
      return UserExperience.Player;
    }
    else if (Enum.TryParse(customAttribute.Value, out UserExperience defaultExperience) && Enum.IsDefined(defaultExperience))
    {
      return defaultExperience;
    }
    throw new ArgumentException($"The user 'Id={user.Id}' default experience '{customAttribute.Value}' is not valid.", nameof(user));
  }

  public static MultiFactorAuthenticationMode GetMultiFactorAuthenticationMode(this User user)
  {
    CustomAttribute? customAttribute = user.GetCustomAttribute(UserHelper.MultiFactorAuthenticationModeKey);
    if (customAttribute is null)
    {
      return MultiFactorAuthenticationMode.None;
    }
    else if (Enum.TryParse(customAttribute.Value, out MultiFactorAuthenticationMode mode) && Enum.IsDefined(mode))
    {
      return mode;
    }
    throw new ArgumentException($"The user 'Id={user.Id}' Multi-Factor Authentication mode '{customAttribute.Value}' is not valid.", nameof(user));
  }

  public static string GetSubject(this User user) => user.Id.ToString();

  public static bool IsProfileCompleted(this User user) => user.GetCustomAttribute(UserHelper.ProfileCompletedOnKey) is not null;

  private static CustomAttribute? GetCustomAttribute(this User user, string key)
  {
    CustomAttribute[] customAttributes = user.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length < 1)
    {
      return null;
    }
    else if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The user 'Id={user.Id}' has multiple ({customAttributes.Length}) custom attributes '{key}'.", nameof(user));
    }
    return customAttributes.Single();
  }
}
