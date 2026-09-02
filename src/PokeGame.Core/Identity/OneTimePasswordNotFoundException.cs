using Logitar;

namespace PokeGame.Core.Identity;

public class OneTimePasswordNotFoundException : IdentityException
{
  private const string ErrorMessage = "The specified One-Time Password (OTP) was not found.";

  public Guid Id
  {
    get => (Guid)Data[nameof(Id)]!;
    private set => Data[nameof(Id)] = value;
  }

  public OneTimePasswordNotFoundException(Guid id) : base(BuildMessage(id))
  {
    Id = id;
  }

  private static string BuildMessage(Guid id) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(Id), id)
    .Build();
}
