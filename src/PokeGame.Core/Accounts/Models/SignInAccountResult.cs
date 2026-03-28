using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Sessions;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountResult
{
  public bool IsPasswordRequired { get; set; }
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

  public static SignInAccountResult RequirePassword() => new()
  {
    IsPasswordRequired = true
  };

  public static SignInAccountResult Success(Session session) => new()
  {
    Session = session
  };
}
