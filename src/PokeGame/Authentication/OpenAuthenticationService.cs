using Krakenar.Contracts.Roles;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;
using PokeGame.Settings;
using Claim = System.Security.Claims.Claim;
using ClaimDto = Krakenar.Contracts.Tokens.Claim;

namespace PokeGame.Authentication;

public interface IOpenAuthenticationService
{
  // TODO(fpion): Task<TokenResponse> GetTokenResponseAsync(Session session, CancellationToken cancellationToken = default);
  // TODO(fpion): Task<TokenResponse> GetTokenResponseAsync(User user, CancellationToken cancellationToken = default);

  Task<User> GetUserAsync(string accessToken, CancellationToken cancellationToken = default);
}

internal class OpenAuthenticationService : IOpenAuthenticationService
{
  private readonly AuthenticationSettings _settings;
  private readonly ITokenService _tokenService;

  public OpenAuthenticationService(AuthenticationSettings settings, ITokenService tokenService)
  {
    _settings = settings;
    _tokenService = tokenService;
  }

  public async Task<User> GetUserAsync(string accessToken, CancellationToken cancellationToken)
  {
    ValidateTokenPayload payload = new(accessToken)
    {
      Type = _settings.AccessToken.Type
    };
    ValidatedToken validatedToken = await _tokenService.ValidateAsync(payload, cancellationToken);
    if (validatedToken.Subject is null)
    {
      throw new ArgumentException("The subject is required.", nameof(accessToken));
    }

    User user = new()
    {
      Id = Guid.Parse(validatedToken.Subject),
      Email = validatedToken.Email
    };

    Guid? sessionId = null;
    foreach (ClaimDto claim in validatedToken.Claims)
    {
      switch (claim.Name)
      {
        case Rfc7519ClaimNames.AuthenticationTime:
          Claim authenticationTime = new Claim(claim.Name, claim.Value, claim.Type);
          user.AuthenticatedOn = ClaimHelper.ExtractDateTime(authenticationTime);
          break;
        case Rfc7519ClaimNames.FullName:
          user.FullName = claim.Value;
          break;
        case Rfc7519ClaimNames.Picture:
          user.Picture = claim.Value;
          break;
        case Rfc7519ClaimNames.Roles:
          user.Roles.Add(new Role(claim.Value));
          break;
        case Rfc7519ClaimNames.SessionId:
          sessionId = Guid.Parse(claim.Value);
          break;
        case Rfc7519ClaimNames.Username:
          user.UniqueName = claim.Value;
          break;
      }
    }
    if (sessionId.HasValue)
    {
      Session session = new()
      {
        Id = sessionId.Value,
        IsActive = true
      };
      if (user.AuthenticatedOn.HasValue)
      {
        session.CreatedOn = session.UpdatedOn = user.AuthenticatedOn.Value;
      }
      user.Sessions.Add(session);
    }

    return user;
  }
}
