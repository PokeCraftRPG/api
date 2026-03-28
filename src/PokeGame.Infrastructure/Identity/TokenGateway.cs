using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class TokenGateway : ITokenGateway
{
  private const string EmailVerificationType = "jwt+verify_email";
  private const string ProfileCompletionType = "jwt+profile";

  private readonly ITokenService _tokenService;

  public TokenGateway(ITokenService tokenService)
  {
    _tokenService = tokenService;
  }

  public async Task<string> CreateEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, TimeSpan.FromDays(7), EmailVerificationType, user: null, new EmailPayload(emailAddress), cancellationToken);
  }
  public async Task<string> CreateEmailVerificationAsync(User user, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, TimeSpan.FromDays(7), EmailVerificationType, user, email: null, cancellationToken);
  }

  public async Task<string> CreateProfileCompletionAsync(User user, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, TimeSpan.FromHours(1), ProfileCompletionType, user, email: null, cancellationToken);
  }

  private async Task<string> CreateAsync(
    bool isConsumable,
    TimeSpan? lifetime,
    string? type,
    User? user,
    EmailPayload? email,
    CancellationToken cancellationToken)
  {
    CreateTokenPayload payload = new()
    {
      IsConsumable = isConsumable,
      LifetimeSeconds = lifetime?.Seconds,
      Type = type,
      Subject = user?.Id.ToString(),
      Email = email ?? (user is null ? null : GetEmailPayload(user))
    };
    CreatedToken created = await _tokenService.CreateAsync(payload, cancellationToken);
    return created.Token;
  }

  private static EmailPayload GetEmailPayload(User user)
  {
    if (user.Email is null)
    {
      throw new ArgumentException("The user does not have an email address.", nameof(user));
    }
    return new EmailPayload(user.Email.Address, user.Email.IsVerified);
  }
}
