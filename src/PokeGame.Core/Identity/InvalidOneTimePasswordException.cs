using Krakenar.Contracts;
using Krakenar.Contracts.Passwords;
using Logitar;

namespace PokeGame.Core.Identity;

public class InvalidOneTimePasswordException : IdentityException
{
  private const string ErrorMessage = "The specified One-Time Password (OTP) purpose was not expected.";

  public Guid OneTimePasswordId
  {
    get => (Guid)Data[nameof(OneTimePasswordId)]!;
    private set => Data[nameof(OneTimePasswordId)] = value;
  }
  public string? AttemptedPurpose
  {
    get => (string?)Data[nameof(AttemptedPurpose)];
    private set => Data[nameof(AttemptedPurpose)] = value;
  }
  public string ExpectedPurpose
  {
    get => (string)Data[nameof(ExpectedPurpose)]!;
    private set => Data[nameof(ExpectedPurpose)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(OneTimePasswordId)] = OneTimePasswordId;
      error.Data[nameof(AttemptedPurpose)] = AttemptedPurpose;
      error.Data[nameof(ExpectedPurpose)] = ExpectedPurpose;
      return error;
    }
  }

  public InvalidOneTimePasswordException(OneTimePassword oneTimePassword, string expectedPurpose) : base(BuildMessage(oneTimePassword, expectedPurpose))
  {
    OneTimePasswordId = oneTimePassword.Id;
    AttemptedPurpose = oneTimePassword.GetPurpose();
    ExpectedPurpose = expectedPurpose;
  }

  private static string BuildMessage(OneTimePassword oneTimePassword, string expectedPurpose) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(OneTimePasswordId), oneTimePassword.Id)
    .AddData(nameof(AttemptedPurpose), oneTimePassword.GetPurpose(), "<null>")
    .AddData(nameof(ExpectedPurpose), expectedPurpose)
    .Build();
}
