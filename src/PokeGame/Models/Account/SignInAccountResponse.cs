using PokeGame.Core.Accounts.Models;

namespace PokeGame.Models.Account;

public record SignInAccountResponse
{
  public bool IsPasswordRequired { get; set; }
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationMessage? MultiFactorAuthenticationMessage { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public CurrentUser? CurrentUser { get; set; }

  public SignInAccountResponse()
  {
  }

  public SignInAccountResponse(SignInAccountResult result)
  {
    IsPasswordRequired = result.IsPasswordRequired;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationMessage = result.MultiFactorAuthenticationMessage;
    ProfileCompletionToken = result.ProfileCompletionToken;

    if (result.Session is not null)
    {
      CurrentUser = new CurrentUser(result.Session);
    }
  }
}
