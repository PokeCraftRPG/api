namespace PokeGame.Core.Identity;

public sealed class OneTimePasswordNotFoundException : IdentityException
{
  public OneTimePasswordNotFoundException(Guid oneTimePasswordId)
    : base("The specified One-Time Password (OTP) was not found.")
  {
    Data["OneTimePasswordId"] = oneTimePasswordId;
  }
}
