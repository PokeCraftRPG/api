using Krakenar.Contracts;
using Krakenar.Contracts.Users;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public class MembershipConflictException : ConflictException
{
  private const string ErrorMessage = "The specified user is already a member of the world.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid? RealmId
  {
    get => (Guid?)Data[nameof(RealmId)];
    private set => Data[nameof(RealmId)] = value;
  }
  public Guid UserId
  {
    get => (Guid)Data[nameof(UserId)]!;
    private set => Data[nameof(UserId)] = value;
  }
  public string EmailAddress
  {
    get => (string)Data[nameof(EmailAddress)]!;
    private set => Data[nameof(EmailAddress)] = value;
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
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(RealmId)] = RealmId;
      error.Data[nameof(UserId)] = UserId;
      error.Data[nameof(EmailAddress)] = EmailAddress;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public MembershipConflictException(World world, User invitee, string emailAddress, string propertyName)
    : base(BuildMessage(world, invitee, emailAddress, propertyName))
  {
    WorldId = world.Id.ToGuid();
    RealmId = invitee.Realm?.Id;
    UserId = invitee.Id;
    EmailAddress = emailAddress;
    PropertyName = propertyName;
  }

  private static string BuildMessage(World world, User invitee, string emailAddress, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), world.Id.ToGuid())
    .AddData(nameof(RealmId), invitee.Realm?.Id, "<null>")
    .AddData(nameof(UserId), invitee.Id)
    .AddData(nameof(EmailAddress), emailAddress)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
