using Krakenar.Contracts;
using Krakenar.Contracts.ApiKeys;

namespace PokeGame.Core.Identity;

public static class ApiKeyExtensions
{
  private const string UserIdKey = "UserId";

  public static Guid GetUserId(this ApiKey apiKey)
  {
    CustomAttribute? customattribute = apiKey.GetCustomAttribute(UserIdKey);
    if (customattribute is null)
    {
      throw new ArgumentException($"The API key 'Id={apiKey.Id}' has no custom attribute '{UserIdKey}'.", nameof(apiKey));
    }
    else if (!Guid.TryParse(customattribute.Value, out Guid userId))
    {
      throw new ArgumentException($"The API key 'Id={apiKey.Id}' custom attribute '{UserIdKey}' is not valid: {customattribute.Value}.", nameof(apiKey));
    }
    else
    {
      return userId;
    }
  }

  private static CustomAttribute? GetCustomAttribute(this ApiKey apiKey, string key)
  {
    CustomAttribute[] customAttributes = apiKey.CustomAttributes.Where(x => x.Key == key).ToArray();
    if (customAttributes.Length < 1)
    {
      return null;
    }
    else if (customAttributes.Length > 1)
    {
      throw new ArgumentException($"The API key 'Id={apiKey.Id}' has multiple ({customAttributes.Length}) custom attributes '{key}'.", nameof(apiKey));
    }
    return customAttributes.Single();
  }
}
