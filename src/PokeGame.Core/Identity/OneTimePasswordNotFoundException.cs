using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Identity;

public class OneTimePasswordNotFoundException : InvalidCredentialsException
{
  private const string ErrorMessage = "The specified One-Time Password (OTP) was not found.";

  public Guid OneTimePasswordId
  {
    get => (Guid)Data[nameof(OneTimePasswordId)]!;
    private set => Data[nameof(OneTimePasswordId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(OneTimePasswordId)] = OneTimePasswordId;
      return error;
    }
  }

  public OneTimePasswordNotFoundException(Guid oneTimePasswordId) : base(BuildMessage(oneTimePasswordId))
  {
    OneTimePasswordId = oneTimePasswordId;
  }

  private static string BuildMessage(Guid oneTimePasswordId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(OneTimePasswordId), oneTimePasswordId)
    .Build();
}
