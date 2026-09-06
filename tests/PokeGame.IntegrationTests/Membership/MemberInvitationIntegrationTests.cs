using FluentValidation;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Membership;

[Trait(Traits.Category, Categories.Integration)]
public class MemberInvitationIntegrationTests : IntegrationTests
{
  private readonly IMembershipService _membershipService;
  private readonly IWorldRepository _worldRepository;

  public MemberInvitationIntegrationTests()
  {
    _membershipService = ServiceProvider.GetRequiredService<IMembershipService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  [Fact(DisplayName = "It should invite a member by email address.")]
  public async Task Given_UnknownEmail_When_Invite_Then_Created()
  {
    SendMemberInvitationPayload payload = CreatePayload();

    MemberInvitationDto invitation = await _membershipService.InviteAsync(payload);

    AssertInvitation(invitation, payload);
    Assert.Equal(ActorType.User, invitation.Invitee.Type);
    Assert.Equal(payload.EmailAddress, invitation.Invitee.EmailAddress);
    Assert.Equal(payload.EmailAddress, invitation.Invitee.DisplayName);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.Is<MemberInvitation>(i => i.EntityId == invitation.Id),
      payload.Locale,
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should invite an existing user.")]
  public async Task Given_ExistingUser_When_Invite_Then_Created()
  {
    User invitee = new UserBuilder(Faker).Build();
    SetupInvitee(invitee);

    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);

    MemberInvitationDto invitation = await _membershipService.InviteAsync(payload);

    AssertInvitation(invitation, payload);
    Assert.Equal(new Actor(invitee), invitation.Invitee);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.Is<MemberInvitation>(i => i.EntityId == invitation.Id && i.UserId == new UserId(invitee)),
      payload.Locale,
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should invite the same email in another world.")]
  public async Task Given_OtherWorld_When_Invite_Then_Created()
  {
    SendMemberInvitationPayload payload = CreatePayload();
    MemberInvitationDto first = await _membershipService.InviteAsync(payload);

    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("other-world").Build();
    await _worldRepository.SaveAsync(world);
    Context.World = world;

    MemberInvitationDto second = await _membershipService.InviteAsync(payload);
    Assert.NotEqual(first.Id, second.Id);
    Assert.Equal(world.EntityId, second.World.Id);
  }

  [Fact(DisplayName = "It should throw MemberInvitationAlreadyPendingException when the email is already invited.")]
  public async Task Given_PendingEmail_When_Invite_Then_MemberInvitationAlreadyPendingException()
  {
    SendMemberInvitationPayload payload = CreatePayload();
    MemberInvitationDto existing = await _membershipService.InviteAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _membershipService.InviteAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(existing.Id, exception.InvitationId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should throw MemberInvitationAlreadyPendingException when the user is already invited.")]
  public async Task Given_PendingUser_When_Invite_Then_MemberInvitationAlreadyPendingException()
  {
    User invitee = new UserBuilder(Faker).Build();
    SetupInvitee(invitee);

    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);
    MemberInvitationDto existing = await _membershipService.InviteAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _membershipService.InviteAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(existing.Id, exception.InvitationId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Invite_Then_ValidationException()
  {
    SendMemberInvitationPayload payload = new();

    await Assert.ThrowsAsync<ValidationException>(async () => await _membershipService.InviteAsync(payload));
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when inviting a member.")]
  public async Task Given_NotAllowed_When_Invite_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    SendMemberInvitationPayload payload = CreatePayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _membershipService.InviteAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("InviteMember", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  private void SetupInvitee(User invitee)
  {
    UserClient.Setup(x => x.ReadAsync(null, invitee.Email!.Address, null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(invitee);
    UserClient.Setup(x => x.SearchAsync(It.IsAny<SearchUsersPayload>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((SearchUsersPayload payload, CancellationToken _) =>
      {
        List<User> users = [];
        if (Context.User is not null && payload.Ids.Contains(Context.User.Id))
        {
          users.Add(Context.User);
        }
        if (payload.Ids.Contains(invitee.Id))
        {
          users.Add(invitee);
        }
        return new SearchResults<User>(users);
      });
  }

  private static SendMemberInvitationPayload CreatePayload(string? emailAddress = null) => new()
  {
    EmailAddress = emailAddress ?? "ash.ketchum@example.com",
    Locale = "en"
  };

  private void AssertInvitation(MemberInvitationDto invitation, SendMemberInvitationPayload payload)
  {
    Assert.NotEqual(Guid.Empty, invitation.Id);
    Assert.Equal(1, invitation.Version);
    Assert.Equal(Actor, invitation.CreatedBy);
    Assert.Equal(DateTime.UtcNow, invitation.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(invitation.CreatedBy, invitation.UpdatedBy);
    Assert.Equal(invitation.CreatedOn, invitation.UpdatedOn, TimeSpan.FromMilliseconds(1));

    Assert.Equal(Context.World!.EntityId, invitation.World.Id);
    Assert.Equal(MemberInvitationStatus.Pending, invitation.Status);
    Assert.NotNull(invitation.ExpiresOn);
    Assert.Equal(DateTime.UtcNow.AddDays(7), invitation.ExpiresOn.Value, TimeSpan.FromSeconds(10));
    Assert.Equal(payload.EmailAddress, invitation.Invitee.EmailAddress);
  }
}
