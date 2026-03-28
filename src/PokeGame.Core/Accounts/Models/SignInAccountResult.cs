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

  public static SignInAccountResult MultiFactorAuthenticationMessageSent(Guid id, MultiFactorAuthenticationMode mode) => new()
  {
    MultiFactorAuthenticationMessage = new MultiFactorAuthenticationMessage(id, mode)
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
