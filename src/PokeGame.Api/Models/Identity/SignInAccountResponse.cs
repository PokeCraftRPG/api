using PokeGame.Core.Identity.Models;

namespace PokeGame.Api.Models.Identity;

public record SignInAccountResponse
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationChallenge? MultiFactorAuthenticationChallenge { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public CurrentUser? CurrentUser { get; set; }

  public SignInAccountResponse()
  {
  }

  public SignInAccountResponse(SignInAccountResult result)
  {
    AllowedFlows = result.AllowedFlows;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationChallenge = result.MultiFactorAuthenticationChallenge;
    ProfileCompletionToken = result.ProfileCompletionToken;

    if (result.Session is not null)
    {
      CurrentUser = new CurrentUser(result.Session);
    }
  }
}
