using PokeGame.Core.Identity.Models;

namespace PokeGame.Api.Models.Identity;

public record GetTokenResponse
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationChallenge? MultiFactorAuthenticationChallenge { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public TokenResponse? Token { get; set; }

  public GetTokenResponse()
  {
  }

  public GetTokenResponse(SignInAccountResult result)
  {
    AllowedFlows = result.AllowedFlows;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationChallenge = result.MultiFactorAuthenticationChallenge;
    ProfileCompletionToken = result.ProfileCompletionToken;
  }
}
