using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using Logitar;

namespace PokeGame.Core.Identity.Models;

public record SignInAccountResult
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationChallenge? MultiFactorAuthenticationChallenge { get; set; }
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

  public static SignInAccountResult MultiFactorAuthenticationMessageSent(OneTimePassword oneTimePassword, Guid messageId, User user)
  {
    MultiFactorAuthenticationMode mode = user.GetMultiFactorAuthenticationMode();
    string maskedContact;
    switch (mode)
    {
      case MultiFactorAuthenticationMode.Email:
        string emailAddress = user.Email?.Address ?? throw new ArgumentException("The user has no email.", nameof(user));
        int index = emailAddress.IndexOf('@');
        maskedContact = index <= 1 ? emailAddress : string.Concat(emailAddress.First(), emailAddress[1..index].Mask('·'), emailAddress[index..]);
        break;
      case MultiFactorAuthenticationMode.Phone:
        string phoneNumber = user.Phone?.E164Formatted ?? throw new ArgumentException("The user has no phone.", nameof(user));
        maskedContact = phoneNumber.Length < 4 ? phoneNumber : string.Concat(phoneNumber[..^4], phoneNumber[^4..].Mask('·'));
        break;
      default:
        maskedContact = string.Empty;
        break;
    }
    return new SignInAccountResult
    {
      MultiFactorAuthenticationChallenge = new MultiFactorAuthenticationChallenge(oneTimePassword, messageId, mode, maskedContact)
    };
  }

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
