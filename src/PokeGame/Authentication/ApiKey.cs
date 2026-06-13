using Krakenar.Contracts.ApiKeys;
using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PokeGame.Core.Identity;
using PokeGame.Extensions;

namespace PokeGame.Authentication;

internal class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
  private readonly IApiKeyGateway _apiKeyGateway;
  private readonly IUserGateway _userGateway;

  public ApiKeyAuthenticationHandler(IApiKeyGateway apiKeyGateway, IUserGateway userGateway, IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : base(options, logger, encoder)
  {
    _apiKeyGateway = apiKeyGateway;
    _userGateway = userGateway;
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (Context.Request.Headers.TryGetValue(Headers.ApiKey, out StringValues xApiKey))
    {
      IReadOnlyCollection<string> sanitized = xApiKey.Sanitize();
      if (sanitized.Count > 1)
      {
        Logger.LogWarning("Multiple {Header} header values were received ({Sanitized} sanitized, {Total} total). Ignoring {Scheme} authentication.",
          Headers.ApiKey, sanitized.Count, xApiKey.Count, Scheme.Name);
      }
      else if (sanitized.Count == 1)
      {
        try
        {
          ApiKey apiKey = await _apiKeyGateway.AuthenticateAsync(sanitized.Single());

          Guid userId = apiKey.GetUserId();
          User? user = await _userGateway.FindAsync(userId);
          if (user is null)
          {
            return AuthenticateResult.Fail($"The user 'Id={userId}' was not found.");
          }

          Context.SetApiKey(apiKey);
          Context.SetUser(user);

          ClaimsPrincipal principal = new(user.CreateClaimsIdentity(Scheme.Name));
          AuthenticationTicket ticket = new(principal, Scheme.Name);

          return AuthenticateResult.Success(ticket);
        }
        catch (Exception exception)
        {
          return AuthenticateResult.Fail(exception);
        }
      }
    }

    return AuthenticateResult.NoResult();
  }
}
