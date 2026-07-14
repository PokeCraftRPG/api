using PokeGame.Core.Identity.Models;

namespace PokeGame.Models.Identity;

public class SignInAccountResponse
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationMessage? MultiFactorAuthenticationMessage { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public CurrentUser? CurrentUser { get; set; }

  public SignInAccountResponse()
  {
  }

  public SignInAccountResponse(SignInAccountResult result)
  {
    AllowedFlows = result.AllowedFlows;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationMessage = result.MultiFactorAuthenticationMessage;
    ProfileCompletionToken = result.ProfileCompletionToken;

    if (result.Session is not null)
    {
      CurrentUser = new CurrentUser(result.Session);
    }
  }
}
