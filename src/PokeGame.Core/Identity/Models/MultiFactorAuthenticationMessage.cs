using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity.Models;

public record MultiFactorAuthenticationMessage
{
  public Guid OneTimePasswordId { get; set; }

  public Guid MessageId { get; set; }
  public MultiFactorAuthenticationMode MultiFactorAuthenticationMode { get; set; }

  public MultiFactorAuthenticationMessage()
  {
  }

  public MultiFactorAuthenticationMessage(OneTimePassword oneTimePassword, Guid messageId, MultiFactorAuthenticationMode multiFactorAuthenticationMode)
  {
    OneTimePasswordId = oneTimePassword.Id;

    MessageId = messageId;
    MultiFactorAuthenticationMode = multiFactorAuthenticationMode;
  }
}
