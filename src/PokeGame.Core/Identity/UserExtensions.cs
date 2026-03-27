using Krakenar.Contracts;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

internal static class UserExtensions
{
  private const string MultiFactorAuthenticationModeKey = "MultiFactorAuthenticationMode";
  private const string ProfileCompletedOnKey = "ProfileCompletedOn";

  public static MultiFactorAuthenticationMode GetMultiFactorAuthenticationMode(this User user)
  {
    CustomAttribute[] customAttributes = user.CustomAttributes.Where(x => x.Key == MultiFactorAuthenticationModeKey).ToArray();
    return customAttributes.Length == 1
      ? Enum.Parse<MultiFactorAuthenticationMode>(customAttributes.Single().Value)
      : MultiFactorAuthenticationMode.None;
  }

  public static bool IsProfileCompleted(this User user)
  {
    CustomAttribute[] customAttributes = user.CustomAttributes.Where(x => x.Key == ProfileCompletedOnKey).ToArray();
    return customAttributes.Length == 1 && DateTime.TryParse(customAttributes.Single().Value, out _);
  }
}
