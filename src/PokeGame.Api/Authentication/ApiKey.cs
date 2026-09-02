using Krakenar.Contracts.ApiKeys;
using Krakenar.Contracts.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PokeGame.Core.Identity;
using PokeGame.Api.Extensions;

namespace PokeGame.Api.Authentication;

internal class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
  private readonly IApiKeyGateway _apiKeyGateway;

  public ApiKeyAuthenticationHandler(IApiKeyGateway apiKeyGateway, IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : base(options, logger, encoder)
  {
    _apiKeyGateway = apiKeyGateway;
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

          Context.SetApiKey(apiKey);

          ClaimsPrincipal principal = new(apiKey.CreateClaimsIdentity(Scheme.Name));
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
