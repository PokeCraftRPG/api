using Bogus;
using Krakenar.Contracts.Users;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

public class SendMembershipInvitationCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMembershipInvitationQuerier> _membershipInvitationQuerier = new();
  private readonly Mock<IMembershipInvitationRepository> _membershipInvitationRepository = new();
  private readonly Mock<IMessageGateway> _messageGateway = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IUserGateway> _userGateway = new();
  private readonly Mock<IWorldRepository> _worldRepository = new();

  private readonly TestContext _context;
  private readonly MembershipSettings _settings;
  private readonly SendMembershipInvitationCommandHandler _handler;

  private readonly World _world;

  public SendMembershipInvitationCommandHandlerTests()
  {
    _context = new(_faker);
    _settings = new()
    {
      InvitationLifetimeDays = 3
    };

    _handler = new(
      _context,
      _membershipInvitationQuerier.Object,
      _membershipInvitationRepository.Object,
      _settings,
      _messageGateway.Object,
      _permissionService.Object,
      _userGateway.Object,
      _worldRepository.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
  }

  [Fact(DisplayName = "It should send a membership invitation to a user.")]
  public async Task Given_UserFound_When_HandleAsync_Then_SentToUser()
  {
    _worldRepository.Setup(x => x.LoadAsync(_world.Id, _cancellationToken)).ReturnsAsync(_world);

    SendMembershipInvitationPayload payload = new()
    {
      Locale = _faker.Locale,
      EmailAddress = _faker.Internet.Email()
    };
    SendMembershipInvitationCommand command = new(payload);

    User user = new UserBuilder(_faker).Build();
    Assert.NotNull(user.Realm);
    _userGateway.Setup(x => x.FindAsync(payload.EmailAddress, _cancellationToken)).ReturnsAsync(user);

    MembershipInvitationModel model = new();
    _membershipInvitationQuerier.Setup(x => x.ReadAsync(It.IsAny<MembershipInvitation>(), _cancellationToken)).ReturnsAsync(model);

    MembershipInvitationModel invitation = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, invitation);

    _permissionService.Verify(x => x.CheckAsync(Actions.SendMembershipInvitation, _cancellationToken), Times.Once());
    _membershipInvitationRepository.Verify(x => x.SaveAsync(
      It.Is<MembershipInvitation>(i => i.Email.Address == payload.EmailAddress && !i.Email.IsVerified
        && i.InviteeId.HasValue && i.InviteeId.Value.RealmId == user.Realm.Id && i.InviteeId.Value.EntityId == user.Id
        && i.Status == MembershipInvitationStatus.Pending && i.ExpiresOn.HasValue && (DateTime.UtcNow - i.ExpiresOn.Value) < TimeSpan.FromSeconds(1)),
      _cancellationToken), Times.Once());
    _membershipInvitationQuerier.Verify(x => x.EnsureNonePendingAsync(It.Is<ReadOnlyEmail>(e => e.Address == payload.EmailAddress && !e.IsVerified), _cancellationToken), Times.Once());
    _messageGateway.Verify(x => x.SendMembershipInvitationAsync(user, payload.Locale, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should send a membership invitation to an email address.")]
  public async Task Given_UserNotFound_When_HandleAsync_Then_SentToEmail()
  {
    SendMembershipInvitationPayload payload = new()
    {
      Locale = _faker.Locale,
      EmailAddress = _faker.Internet.Email()
    };
    SendMembershipInvitationCommand command = new(payload);

    MembershipInvitationModel model = new();
    _membershipInvitationQuerier.Setup(x => x.ReadAsync(It.IsAny<MembershipInvitation>(), _cancellationToken)).ReturnsAsync(model);

    MembershipInvitationModel invitation = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, invitation);

    _permissionService.Verify(x => x.CheckAsync(Actions.SendMembershipInvitation, _cancellationToken), Times.Once());
    _membershipInvitationRepository.Verify(x => x.SaveAsync(
      It.Is<MembershipInvitation>(i => i.Email.Address == payload.EmailAddress && !i.Email.IsVerified && !i.InviteeId.HasValue
        && i.Status == MembershipInvitationStatus.Pending && i.ExpiresOn.HasValue && (DateTime.UtcNow - i.ExpiresOn.Value) < TimeSpan.FromSeconds(1)),
      _cancellationToken), Times.Once());
    _membershipInvitationQuerier.Verify(x => x.EnsureNonePendingAsync(It.Is<ReadOnlyEmail>(e => e.Address == payload.EmailAddress && !e.IsVerified), _cancellationToken), Times.Once());
    _messageGateway.Verify(x => x.SendMembershipInvitationAsync(It.Is<ReadOnlyEmail>(e => e.Address == payload.EmailAddress && !e.IsVerified), payload.Locale, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when the world was not loaded.")]
  public async Task Given_WorldNotLoaded_When_HandleAsync_Then_InvalidOperationException()
  {
    User invitee = new UserBuilder().Build();
    Assert.NotNull(invitee.Email);
    _userGateway.Setup(x => x.FindAsync(invitee.Email.Address, _cancellationToken)).ReturnsAsync(invitee);

    SendMembershipInvitationPayload payload = new()
    {
      Locale = _faker.Locale,
      EmailAddress = invitee.Email.Address
    };
    SendMembershipInvitationCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal($"The world 'Id={_context.WorldId}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "It should throw MembershipConflictException when the user is already a member.")]
  public async Task Given_AlreadyMember_When_HandleAsync_Then_MembershipConflictException()
  {
    _worldRepository.Setup(x => x.LoadAsync(_world.Id, _cancellationToken)).ReturnsAsync(_world);

    User member = new UserBuilder().Build();
    Assert.NotNull(member.Email);
    _userGateway.Setup(x => x.FindAsync(member.Email.Address, _cancellationToken)).ReturnsAsync(member);

    _world.GrantMembership(member.GetUserId(), _world.OwnerId);

    SendMembershipInvitationPayload payload = new()
    {
      Locale = _faker.Locale,
      EmailAddress = member.Email.Address
    };
    SendMembershipInvitationCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<MembershipConflictException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(member.Realm?.Id, exception.RealmId);
    Assert.Equal(member.Id, exception.UserId);
    Assert.Equal(payload.EmailAddress, exception.EmailAddress);
    Assert.Equal("EmailAddress", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    SendMembershipInvitationPayload payload = new()
    {
      Locale = "invalid",
      EmailAddress = "aa@@bb..cc"
    };
    SendMembershipInvitationCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LocaleValidator" && e.PropertyName == "Locale");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EmailValidator" && e.PropertyName == "EmailAddress");
  }
}
