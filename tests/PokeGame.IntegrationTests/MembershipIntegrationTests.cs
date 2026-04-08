using Krakenar.Client;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Messages;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Actors;
using PokeGame.Core.Caching;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class MembershipIntegrationTests : IntegrationTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly ICacheService _cacheService;
  private readonly IMembershipInvitationRepository _membershipInvitationRepository;
  private readonly IMembershipService _membershipService;
  private readonly IWorldQuerier _worldQuerier;
  private readonly IWorldRepository _worldRepository;

  private readonly User _user;

  public MembershipIntegrationTests() : base()
  {
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _membershipInvitationRepository = ServiceProvider.GetRequiredService<IMembershipInvitationRepository>();
    _membershipService = ServiceProvider.GetRequiredService<IMembershipService>();
    _worldQuerier = ServiceProvider.GetRequiredService<IWorldQuerier>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();

    _user = new UserBuilder().Build();
    _cacheService.SetActor(new Actor(_user));
  }

  [Fact(DisplayName = "It should accept a membership invitation.")]
  public async Task Given_Invitation_When_AcceptInvitation_Then_Accepted()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithInvitee(_user).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    User? owner = Context.User;
    Assert.NotNull(owner);
    Context.User = _user;

    MembershipInvitationModel? model = await _membershipService.AcceptInvitationAsync(invitation.EntityId);
    Assert.NotNull(model);
    Assert.Equal(2, model.Version);
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MembershipInvitationStatus.Accepted, model.Status);

    Context.User = owner;

    WorldModel world = await _worldQuerier.ReadAsync(World);
    Assert.Equal(new Actor(owner), world.Owner);
    Assert.Single(world.Membership);
    Assert.Contains(world.Membership, m => m.Member.Equals(new Actor(_user))
      && m.GrantedBy.Equals(new Actor(_user)) && (DateTime.UtcNow - m.GrantedOn) < TimeSpan.FromSeconds(10)
      && m.RevokedBy is null && !m.RevokedOn.HasValue);
  }

  [Fact(DisplayName = "It should cancel a membership invitation.")]
  public async Task Given_Invitation_When_CancelInvitation_Then_Cancelled()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithInvitee(_user).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    MembershipInvitationModel? model = await _membershipService.CancelInvitationAsync(invitation.EntityId);
    Assert.NotNull(model);
    Assert.Equal(2, model.Version);
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MembershipInvitationStatus.Cancelled, model.Status);
  }

  [Fact(DisplayName = "It should decline a membership invitation.")]
  public async Task Given_Invitation_When_DeclineInvitation_Then_Declined()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithInvitee(_user).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    User? owner = Context.User;
    Assert.NotNull(owner);
    Context.User = _user;

    MembershipInvitationModel? model = await _membershipService.DeclineInvitationAsync(invitation.EntityId);
    Assert.NotNull(model);
    Assert.Equal(2, model.Version);
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MembershipInvitationStatus.Declined, model.Status);

    Context.User = owner;

    WorldModel world = await _worldQuerier.ReadAsync(World);
    Assert.Equal(new Actor(owner), world.Owner);
    Assert.Empty(world.Membership);
  }

  [Fact(DisplayName = "It should read an invitation by ID (Invitee).")]
  public async Task Given_Id_When_ReadInvitation_Then_ReadAsInvitee()
  {
    Assert.NotNull(_user.Email);
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithInvitee(_user).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    Context.User = _user;

    MembershipInvitationModel? model = await _membershipService.ReadInvitationAsync(invitation.EntityId);
    Assert.NotNull(model);
    Assert.Equal(invitation.EntityId, model.Id);
    Assert.Equal(new Actor(_user), model.Invitee);
  }

  [Theory(DisplayName = "It should read an invitation by ID (Owner).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_Id_When_ReadInvitation_Then_ReadAsOwner(bool withInvitee)
  {
    Assert.NotNull(_user.Email);
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithEmail(_user.Email).WithInvitee(withInvitee ? _user : null).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    MembershipInvitationModel? model = await _membershipService.ReadInvitationAsync(invitation.EntityId);
    Assert.NotNull(model);
    Assert.Equal(invitation.EntityId, model.Id);
    if (withInvitee)
    {
      Assert.Equal(new Actor(_user), model.Invitee);
    }
    else
    {
      Assert.Equal(_user.Email.Address, model.EmailAddress);
    }
  }

  [Fact(DisplayName = "It should revoke a membership.")]
  public async Task Given_Membership_When_Revoke_Then_Revoked()
  {
    World.GrantMembership(_user.GetUserId(), World.OwnerId);
    await _worldRepository.SaveAsync(World);

    WorldModel world = await _membershipService.RevokeAsync(_user.Id);

    Assert.Equal(World.Id.ToGuid(), world.Id);
    Assert.Equal(Actor, world.Owner);

    Assert.Single(world.Membership);
    Assert.Contains(world.Membership, m => m.Member.Equals(new Actor(_user))
      && m.GrantedBy.Equals(Actor) && (DateTime.UtcNow - m.GrantedOn) < TimeSpan.FromSeconds(10)
      && m.RevokedBy is not null && m.RevokedBy.Equals(Actor) && m.RevokedOn.HasValue && (DateTime.UtcNow - m.RevokedOn) < TimeSpan.FromSeconds(10));
  }

  [Fact(DisplayName = "It should send an invitation to a user.")]
  public async Task Given_UserFound_When_SendInvitation_Then_InvitationSentToEmail()
  {
    Assert.NotNull(_user.Email);
    UserClient.Setup(x => x.ReadAsync(id: null, _user.Email.Address, customIdentifier: null, It.IsAny<RequestContext>())).ReturnsAsync(_user);

    SendMembershipInvitationPayload payload = new()
    {
      Locale = Faker.Locale,
      EmailAddress = _user.Email.Address
    };

    MembershipInvitationModel model = await _membershipService.SendInvitationAsync(payload, _cancellationToken);

    Assert.Equal(1, model.Version);
    Assert.Equal(Actor, model.CreatedBy);
    Assert.Equal(DateTime.UtcNow, model.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, model.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, model.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.EmailAddress, model.EmailAddress);
    Assert.Equal(new Actor(_user), model.Invitee);
    Assert.Equal(MembershipInvitationStatus.Pending, model.Status);
    Assert.NotNull(model.ExpiresOn);
    Assert.Equal(DateTime.UtcNow.AddDays(7), model.ExpiresOn.Value, TimeSpan.FromSeconds(10));

    MessageService.Verify(x => x.SendAsync(
      It.Is<SendMessagePayload>(p => p.Sender == "Email" && p.Template == "MembershipInvitation"
        && IsRecipient(Assert.Single(p.Recipients), _user)
        && !p.IgnoreUserLocale && p.Locale == payload.Locale && !p.Variables.Any()
        && !p.IsDemo),
      _cancellationToken), Times.Once());
  }
  private static bool IsRecipient(RecipientPayload recipient, User invitee) => recipient.Type == RecipientType.To
    && recipient.Email is null
    && recipient.Phone is null
    && recipient.DisplayName is null
    && recipient.UserId == invitee.Id;

  [Fact(DisplayName = "It should send an invitation to an email address when the user was not found.")]
  public async Task Given_UserNotFound_When_SendInvitation_Then_InvitationSentToEmail()
  {
    SendMembershipInvitationPayload payload = new()
    {
      Locale = Faker.Locale,
      EmailAddress = Faker.Internet.Email()
    };

    MembershipInvitationModel model = await _membershipService.SendInvitationAsync(payload, _cancellationToken);

    Assert.Equal(payload.EmailAddress, model.EmailAddress);
    Assert.Null(model.Invitee);
    Assert.Equal(MembershipInvitationStatus.Pending, model.Status);
    Assert.NotNull(model.ExpiresOn);
    Assert.Equal(DateTime.UtcNow.AddDays(7), model.ExpiresOn.Value, TimeSpan.FromSeconds(10));

    MessageService.Verify(x => x.SendAsync(
      It.Is<SendMessagePayload>(p => p.Sender == "Email" && p.Template == "MembershipInvitation"
        && IsRecipient(Assert.Single(p.Recipients), payload.EmailAddress)
        && !p.IgnoreUserLocale && p.Locale == payload.Locale && !p.Variables.Any()
        && !p.IsDemo),
      _cancellationToken), Times.Once());
  }
  private static bool IsRecipient(RecipientPayload recipient, string emailAddress) => recipient.Type == RecipientType.To
    && recipient.Email is not null && recipient.Email.Address == emailAddress
    && recipient.Phone is null
    && recipient.DisplayName is null
    && !recipient.UserId.HasValue;

  [Theory(DisplayName = "It should throw MembershipInvitationPendingException when there are pending invitations for the email.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_PendingInvitation_When_SendInvitation_Then_MembershipInvitationPendingException(bool withInvitee)
  {
    Assert.NotNull(_user.Email);
    MembershipInvitation invitation = new MembershipInvitationBuilder(Faker).WithWorld(World).WithEmail(_user.Email).WithInvitee(withInvitee ? _user : null).Build();
    await _membershipInvitationRepository.SaveAsync(invitation);

    SendMembershipInvitationPayload payload = new()
    {
      Locale = Faker.Locale,
      EmailAddress = _user.Email.Address
    };

    var exception = await Assert.ThrowsAsync<MembershipInvitationPendingException>(async () => await _membershipService.SendInvitationAsync(payload));
    Assert.Equal(_user.Email.Address, exception.EmailAddress);
  }
}
