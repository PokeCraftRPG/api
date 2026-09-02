using PokeGame.Core.Identity;
using PokeGame.Core.Identity.Models;
using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Roles;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using Logitar.Security.Claims;
using Claim = System.Security.Claims.Claim;
using ClaimDto = Krakenar.Contracts.Tokens.Claim;

namespace PokeGame.Infrastructure.Identity;

internal class TokenGateway : ITokenGateway
{
  private const string AccessTokenType = "at+jwt";
  private const string EmailVerificationType = "verify_email+jwt";
  private const string ProfileCompletionType = "profile+jwt";

  private readonly TokensSettings _settings;
  private readonly ITokenService _tokenService;

  public TokenGateway(TokensSettings settings, ITokenService tokenService)
  {
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

  public async Task<string> CreateEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, _settings.EmailVerification.LifetimeSeconds, EmailVerificationType, user: null, new EmailPayload(emailAddress), cancellationToken);
  }
  public async Task<string> CreateEmailVerificationAsync(User user, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, _settings.EmailVerification.LifetimeSeconds, EmailVerificationType, user, email: null, cancellationToken);
  }
  public async Task<ValidatedToken> ValidateEmailVerificationAsync(string token, CancellationToken cancellationToken)
  {
    ValidateTokenPayload payload = new(token)
    {
      Consume = true,
      Type = EmailVerificationType
    };
    return await _tokenService.ValidateAsync(payload, cancellationToken);
  }

  public async Task<string> CreateProfileCompletionAsync(User user, CancellationToken cancellationToken)
  {
    return await CreateAsync(isConsumable: true, _settings.ProfileCompletion.LifetimeSeconds, ProfileCompletionType, user, email: null, cancellationToken);
  }
  public async Task<ValidatedToken> ValidateProfileCompletionAsync(string token, CancellationToken cancellationToken)
  {
    ValidateTokenPayload payload = new(token)
    {
      Consume = true,
      Type = ProfileCompletionType
    };
    return await _tokenService.ValidateAsync(payload, cancellationToken);
  }

  private async Task<string> CreateAsync(
    bool isConsumable,
    int? lifetimeSeconds,
    string? type,
    User? user,
    EmailPayload? email,
    CancellationToken cancellationToken)
  {
    CreateTokenPayload payload = new()
    {
      IsConsumable = isConsumable,
      LifetimeSeconds = lifetimeSeconds,
      Type = type,
      Subject = user?.GetSubject(),
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
