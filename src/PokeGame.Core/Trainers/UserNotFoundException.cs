using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Trainers;

public class UserNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified user was not found.";

  public Guid UserId
  {
    get => (Guid)Data[nameof(UserId)]!;
    private set => Data[nameof(UserId)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(UserId)] = UserId;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public UserNotFoundException(Guid userId, string propertyName) : base(BuildMessage(userId, propertyName))
  {
    UserId = userId;
    PropertyName = propertyName;
  }

  private static string BuildMessage(Guid userId, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(UserId), userId)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
