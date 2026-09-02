using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity.Models;

public record MultiFactorAuthenticationChallenge
{
  public Guid OneTimePasswordId { get; set; }
  public Guid MessageId { get; set; }
  public MultiFactorAuthenticationMode Mode { get; set; }
  public string MaskedContact { get; set; } = string.Empty;

  public MultiFactorAuthenticationChallenge()
  {
  }

  public MultiFactorAuthenticationChallenge(OneTimePassword oneTimePassword, Guid messageId, MultiFactorAuthenticationMode mode, string maskedContact)
  {
    OneTimePasswordId = oneTimePassword.Id;
    MessageId = messageId;
    Mode = mode;
    MaskedContact = maskedContact;
  }
}
