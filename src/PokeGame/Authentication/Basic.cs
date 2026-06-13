using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PokeGame.Core.Identity;
using PokeGame.Extensions;

namespace PokeGame.Authentication;

internal class BasicAuthenticationOptions : AuthenticationSchemeOptions;

internal class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
  private readonly IUserGateway _userGateway;

  public BasicAuthenticationHandler(IUserGateway userGateway, IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : base(options, logger, encoder)
  {
    _userGateway = userGateway;
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (Context.Request.Headers.TryGetValue(Headers.Authorization, out StringValues authorization))
    {
      IReadOnlyCollection<string> sanitized = authorization.Sanitize();
      if (sanitized.Count > 1)
      {
        Logger.LogWarning("Multiple {Header} header values were received ({Sanitized} sanitized, {Total} total). Ignoring {Scheme} authentication.",
          Headers.Authorization, sanitized.Count, authorization.Count, Scheme.Name);
      }
      else if (sanitized.Count == 1)
      {
        string value = sanitized.Single();
        string[] values = value.Split();
        if (values.Length != 2)
        {
          return AuthenticateResult.Fail($"The Authorization header value is not valid: '{value}'.");
        }
        else if (values[0] == Schemes.Basic)
        {
          byte[] bytes = Convert.FromBase64String(values[1]);
          string credentials = Encoding.UTF8.GetString(bytes);
          int index = credentials.IndexOf(':');
          if (index <= 0)
          {
            return AuthenticateResult.Fail($"The Basic credentials are not valid: '{credentials}'.");
          }

          try
          {
            string uniqueName = credentials[..index];
            string password = credentials[(index + 1)..];
            User user = await _userGateway.AuthenticateAsync(uniqueName, password);

            MultiFactorAuthenticationMode mode = user.GetMultiFactorAuthenticationMode();
            if (mode != MultiFactorAuthenticationMode.None)
            {
              return AuthenticateResult.Fail($"The user 'Id={user.Id}' Multi-Factor Authentication mode is '{mode}'.");
            }

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
    }

    return AuthenticateResult.NoResult();
  }
}
