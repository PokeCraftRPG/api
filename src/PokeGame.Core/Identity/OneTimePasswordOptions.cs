namespace PokeGame.Core.Identity;

public record OneTimePasswordOptions(string Characters, int Length = 6, TimeSpan? ExpiresOn = null, int? MaximumAttempts = null)
{
  public static readonly OneTimePasswordOptions MultiFactorAuthentication = new("0123456789", Length: 6, TimeSpan.FromHours(1), MaximumAttempts: 5);
}
