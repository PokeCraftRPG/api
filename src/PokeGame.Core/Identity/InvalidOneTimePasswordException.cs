using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;
using Logitar;

namespace PokeGame.Core.Identity;

public class InvalidOneTimePasswordException : InvalidCredentialsException
{
  private const string ErrorMessage = "The specified One-Time Password (OTP) purpose was not expected.";

  public Guid OneTimePasswordId
  {
    get => (Guid)Data[nameof(OneTimePasswordId)]!;
    private set => Data[nameof(OneTimePasswordId)] = value;
  }
  public string? ExpectedPurpose
  {
    get => (string?)Data[nameof(ExpectedPurpose)];
    private set => Data[nameof(ExpectedPurpose)] = value;
  }
  public string AttemptedPurpose
  {
    get => (string)Data[nameof(AttemptedPurpose)]!;
    private set => Data[nameof(AttemptedPurpose)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(OneTimePasswordId)] = OneTimePasswordId;
      error.Data[nameof(ExpectedPurpose)] = ExpectedPurpose;
      error.Data[nameof(AttemptedPurpose)] = AttemptedPurpose;
      return error;
    }
  }

  public InvalidOneTimePasswordException(OneTimePassword oneTimePassword, string attemptedPurpose) : base(BuildMessage(oneTimePassword, attemptedPurpose))
  {
    OneTimePasswordId = oneTimePassword.Id;
    ExpectedPurpose = oneTimePassword?.GetPurpose();
    AttemptedPurpose = attemptedPurpose;
  }

  private static string BuildMessage(OneTimePassword oneTimePassword, string attemptedPurpose) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(OneTimePasswordId), oneTimePassword.Id)
    .AddData(nameof(ExpectedPurpose), oneTimePassword?.GetPurpose(), "<null>")
    .AddData(nameof(AttemptedPurpose), attemptedPurpose)
    .Build();
}
