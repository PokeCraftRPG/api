using PokeGame.Core.Accounts.Models;

namespace PokeGame.Models.Account;

public record GetTokenResponse
{
  public bool IsPasswordRequired { get; set; }
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationMessage? MultiFactorAuthenticationMessage { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public TokenResponse? Token { get; set; }

  public GetTokenResponse()
  {
  }

  public GetTokenResponse(SignInAccountResult result)
  {
    IsPasswordRequired = result.IsPasswordRequired;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationMessage = result.MultiFactorAuthenticationMessage;
    ProfileCompletionToken = result.ProfileCompletionToken;
  }
}
