using Krakenar.Contracts.Passwords;

namespace PokeGame.Core.Identity;

public sealed class InvalidOneTimePasswordException : IdentityException
{
  public InvalidOneTimePasswordException(OneTimePassword oneTimePassword, string expectedPurpose)
    : base("The specified One-Time Password (OTP) purpose was not expected.")
  {
    Data["OneTimePasswordId"] = oneTimePassword.Id;
    Data["AttemptedPurpose"] = oneTimePassword.GetPurpose();
    Data["ExpectedPurpose"] = expectedPurpose;
  }
}
