using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Identity;

public sealed class OneTimePasswordNotFoundException : IdentityException
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

  public OneTimePasswordNotFoundException(Guid id) : base(BuildMessage(id))
  {
    OneTimePasswordId = id;
  }

  private static string BuildMessage(Guid id) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(OneTimePasswordId), id)
    .Build();
}
