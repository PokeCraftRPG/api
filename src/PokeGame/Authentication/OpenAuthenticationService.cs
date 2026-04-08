using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Roles;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;
using PokeGame.Core.Identity;
using PokeGame.Models.Account;
using PokeGame.Settings;
using Claim = System.Security.Claims.Claim;
using ClaimDto = Krakenar.Contracts.Tokens.Claim;

namespace PokeGame.Authentication;

public interface IOpenAuthenticationService
{
  Task<TokenResponse> GetTokenResponseAsync(Session session, CancellationToken cancellationToken = default);
  Task<TokenResponse> GetTokenResponseAsync(User user, CancellationToken cancellationToken = default);

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

  public async Task<TokenResponse> GetTokenResponseAsync(Session session, CancellationToken cancellationToken)
  {
    return await GetTokenResponseAsync(session.User, session, cancellationToken);
  }
  public async Task<TokenResponse> GetTokenResponseAsync(User user, CancellationToken cancellationToken)
  {
    return await GetTokenResponseAsync(user, session: null, cancellationToken);
  }
  private async Task<TokenResponse> GetTokenResponseAsync(User user, Session? session, CancellationToken cancellationToken)
  {
    int lifetimeSeconds = _settings.AccessToken.LifetimeSeconds;

    CreateTokenPayload payload = new()
    {
      LifetimeSeconds = lifetimeSeconds,
      Type = _settings.AccessToken.Type,
      Subject = user.GetSubject()
    };
    if (user.Email is not null)
    {
      payload.Email = new EmailPayload(user.Email.Address, user.Email.IsVerified);
    }
    payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.Username, user.UniqueName));
    if (user.FullName is not null)
    {
      payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.FullName, user.FullName));
    }
    if (user.Picture is not null)
    {
      payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.Picture, user.Picture));
    }
    foreach (Role role in user.Roles)
    {
      payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.Roles, role.UniqueName));
    }

    if (session is not null)
    {
      payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.SessionId, session.Id.ToString()));
    }

    CreatedToken created = await _tokenService.CreateAsync(payload, cancellationToken);
    return new TokenResponse(Schemes.Bearer, created.Token)
    {
      ExpiresIn = lifetimeSeconds,
      RefreshToken = session?.RefreshToken
    };
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
