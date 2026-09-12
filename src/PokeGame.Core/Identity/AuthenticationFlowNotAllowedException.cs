using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public sealed class AuthenticationFlowNotAllowedException : IdentityException
{

  public AuthenticationFlowNotAllowedException(AuthenticationFlow authenticationFlow)
    : base("The specified authentication flow is not allowed.")
  {
    Data["AuthenticationFlow"] = authenticationFlow;
  }

  public static AuthenticationFlowNotAllowedException Passwordless => new(AuthenticationFlow.Passwordless);
}
