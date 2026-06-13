using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Realms;
using Krakenar.Contracts.Roles;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;
using Claim = System.Security.Claims.Claim;
using ClaimDto = Krakenar.Contracts.Tokens.Claim;

namespace PokeGame.Infrastructure.Identity;

internal class TokenGateway : ITokenGateway
{
  private const string AccessTokenType = "at+jwt";

  private readonly ICacheService _cacheService;
  private readonly TokensSettings _settings;
  private readonly ITokenService _tokenService;

  public TokenGateway(ICacheService cacheService, TokensSettings settings, ITokenService tokenService)
  {
    _cacheService = cacheService;
    _settings = settings;
    _tokenService = tokenService;
  }

  public async Task<TokenResponse> GetResponseAsync(Session session, CancellationToken cancellationToken)
  {
    User user = session.User;
    int lifetimeSeconds = _settings.Access.LifetimeSeconds;

    CreateTokenPayload payload = new()
    {
      LifetimeSeconds = lifetimeSeconds,
      Type = AccessTokenType,
      Subject = user.GetSubject()
    };
    payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.Username, user.UniqueName));
    if (user.Email is not null)
    {
      payload.Email = new EmailPayload(user.Email.Address, user.Email.IsVerified);
    }
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

    payload.Claims.Add(new ClaimDto(Rfc7519ClaimNames.SessionId, session.Id.ToString()));

    DateTime authenticationTime = user.AuthenticatedOn ?? session.CreatedOn;
    Claim claim = ClaimHelper.Create(Rfc7519ClaimNames.AuthenticationTime, authenticationTime);
    payload.Claims.Add(new ClaimDto(claim.Type, claim.Value, claim.ValueType));

    CreatedToken access = await _tokenService.CreateAsync(payload, cancellationToken);
    return new TokenResponse(Schemes.Bearer, access.Token)
    {
      ExpiresIn = lifetimeSeconds,
      RefreshToken = session.RefreshToken
    };
  }
  public async Task<User> ValidateAccessAsync(string token, CancellationToken cancellationToken)
  {
    ValidateTokenPayload payload = new(token)
    {
      Type = AccessTokenType
    };
    ValidatedToken validatedToken = await _tokenService.ValidateAsync(payload, cancellationToken);
    if (validatedToken.Subject is null)
    {
      throw new ArgumentException("The subject is required.", nameof(token));
    }

    User user = new()
    {
      Id = Guid.Parse(validatedToken.Subject),
      Realm = new Realm
      {
        Id = _cacheService.RealmId
      },
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
