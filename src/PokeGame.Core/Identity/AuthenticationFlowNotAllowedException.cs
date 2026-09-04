using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public sealed class AuthenticationFlowNotAllowedException : IdentityException
{
  private const string ErrorMessage = "The specified authentication flow is not allowed.";

  public AuthenticationFlow AuthenticationFlow
  {
    get => (AuthenticationFlow)Data[nameof(AuthenticationFlow)]!;
    private set => Data[nameof(AuthenticationFlow)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(AuthenticationFlow)] = AuthenticationFlow;
      return error;
    }
  }

  public AuthenticationFlowNotAllowedException(AuthenticationFlow authenticationFlow) : base(BuildMessage(authenticationFlow))
  {
    AuthenticationFlow = authenticationFlow;
  }

  public static AuthenticationFlowNotAllowedException Passwordless => new(AuthenticationFlow.Passwordless);

  private static string BuildMessage(AuthenticationFlow authenticationFlow) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(AuthenticationFlow), authenticationFlow)
    .Build();
}
