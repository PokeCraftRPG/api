using PokeGame.Core.Identity.Models;

namespace PokeGame.Models.Identity;

public class GetTokenResponse
{
  public List<AuthenticationFlow> AllowedFlows { get; set; } = [];
  public Guid? EmailVerificationMessageId { get; set; }
  public MultiFactorAuthenticationMessage? MultiFactorAuthenticationMessage { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public TokenResponse? Token { get; set; }

  public GetTokenResponse()
  {
  }

  public GetTokenResponse(SignInAccountResult result)
  {
    AllowedFlows = result.AllowedFlows;
    EmailVerificationMessageId = result.EmailVerificationMessageId;
    MultiFactorAuthenticationMessage = result.MultiFactorAuthenticationMessage;
    ProfileCompletionToken = result.ProfileCompletionToken;
  }
}
