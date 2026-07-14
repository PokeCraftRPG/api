using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Sessions;

namespace PokeGame.Core.Identity.Models;

public record SignInAccountResult
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationMessage? MultiFactorAuthenticationMessage { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public Session? Session { get; set; }

  public static SignInAccountResult CompleteProfile(string token) => new()
  {
    ProfileCompletionToken = token
  };

  public static SignInAccountResult EmailVerificationMessageSent(Guid id) => new()
  {
    EmailVerificationMessageId = id
  };

  public static SignInAccountResult MultiFactorAuthenticationMessageSent(OneTimePassword oneTimePassword, Guid messageId, MultiFactorAuthenticationMode multiFactorAuthenticationMode) => new()
  {
    MultiFactorAuthenticationMessage = new MultiFactorAuthenticationMessage(oneTimePassword, messageId, multiFactorAuthenticationMode)
  };

  public static SignInAccountResult RequirePassword(bool allowPasswordless = false)
  {
    SignInAccountResult result = new();
    result.AllowedFlows.Add(AuthenticationFlow.Password);
    if (allowPasswordless)
    {
      result.AllowedFlows.Add(AuthenticationFlow.Passwordless);
    }
    return result;
  }

  public static SignInAccountResult Succeed(Session session) => new()
  {
    Session = session
  };
}
