using FluentValidation;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure;

namespace PokeGame.Membership;

[Trait(Traits.Category, Categories.Integration)]
public class MemberInvitationIntegrationTests : IntegrationTests
{
  private readonly IMemberInvitationService _memberInvitationService;
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  public MemberInvitationIntegrationTests()
  {
    _memberInvitationService = ServiceProvider.GetRequiredService<IMemberInvitationService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  [Fact(DisplayName = "It should invite a member by email address.")]
  public async Task Given_UnknownEmail_When_Invite_Then_Created()
  {
    SendMemberInvitationPayload payload = CreatePayload();

    MemberInvitationDto invitation = await _memberInvitationService.SendAsync(payload);

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

    MemberInvitationDto invitation = await _memberInvitationService.SendAsync(payload);

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
    MemberInvitationDto first = await _memberInvitationService.SendAsync(payload);

    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("other-world").Build();
    await _worldRepository.SaveAsync(world);
    Context.World = world;

    MemberInvitationDto second = await _memberInvitationService.SendAsync(payload);
    Assert.NotEqual(first.Id, second.Id);
    Assert.Equal(world.EntityId, second.World.Id);
  }

  [Fact(DisplayName = "It should throw MemberAlreadyExistsException when the user is already a member.")]
  public async Task Given_ExistingMember_When_Invite_Then_MemberAlreadyExistsException()
  {
    User member = Context.User!;
    SetupInvitee(member);

    SendMemberInvitationPayload payload = CreatePayload(member.Email!.Address);

    MemberAlreadyExistsException exception = await Assert.ThrowsAsync<MemberAlreadyExistsException>(
      async () => await _memberInvitationService.SendAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(member.Id, exception.UserId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should throw MemberInvitationAlreadyPendingException when the email is already invited.")]
  public async Task Given_PendingEmail_When_Invite_Then_MemberInvitationAlreadyPendingException()
  {
    SendMemberInvitationPayload payload = CreatePayload();
    MemberInvitationDto existing = await _memberInvitationService.SendAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _memberInvitationService.SendAsync(payload));
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
    MemberInvitationDto existing = await _memberInvitationService.SendAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _memberInvitationService.SendAsync(payload));
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

    await Assert.ThrowsAsync<ValidationException>(async () => await _memberInvitationService.SendAsync(payload));
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
      async () => await _memberInvitationService.SendAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("InviteMember", exception.Action);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should read a member invitation by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    MemberInvitationDto seeded = await _memberInvitationService.SendAsync(CreatePayload());

    MemberInvitationDto? invitation = await _memberInvitationService.ReadAsync(seeded.Id);
    Assert.NotNull(invitation);
    Assert.Equal(seeded.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should return null when no invitation was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    MemberInvitationDto seeded = await _memberInvitationService.SendAsync(CreatePayload());
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _memberInvitationService.ReadAsync(seeded.Id));
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    await _memberInvitationService.SendAsync(CreatePayload());
    Context.World = new WorldBuilder(Faker).Build();

    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    MemberInvitationDto ash = await _memberInvitationService.SendAsync(CreatePayload("ash.ketchum@example.com"));
    MemberInvitationDto misty = await _memberInvitationService.SendAsync(CreatePayload("misty.waterflower@example.com"));
    await _memberInvitationService.SendAsync(CreatePayload("brock.harrison@example.com"));
    await _memberInvitationService.CancelAsync(misty.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("ash");
    payload.Search.Terms.Add("misty");
    payload.Ids.AddRange([ash.Id, misty.Id]);
    payload.Sort.Add(new SortOption<MemberInvitationSort>(MemberInvitationSort.UpdatedOn, SortDirection.Descending));

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(ash.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should filter search results by status.")]
  public async Task Given_StatusFilter_When_Search_Then_Results()
  {
    await _memberInvitationService.SendAsync(CreatePayload("pending@example.com"));
    MemberInvitationDto cancelled = await _memberInvitationService.SendAsync(CreatePayload("cancelled@example.com"));
    await _memberInvitationService.CancelAsync(cancelled.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      Status = MemberInvitationStatus.Cancelled,
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(cancelled.Id, invitation.Id);
    Assert.Equal(MemberInvitationStatus.Cancelled, invitation.Status);
  }

  [Theory(DisplayName = "It should filter search results by expiration.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_ExpiredFilter_When_Search_Then_Results(bool isExpired)
  {
    MemberInvitationDto active = await _memberInvitationService.SendAsync(CreatePayload("active@example.com"));
    MemberInvitationDto expired = await _memberInvitationService.SendAsync(CreatePayload("expired@example.com"));
    await ExpireInvitationAsync(expired.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      IsExpired = isExpired,
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(isExpired ? expired.Id : active.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should accept a member invitation.")]
  public async Task Given_Pending_When_Accept_Then_Accepted()
  {
    User owner = Context.User!;
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    MemberInvitationDto? invitation = await _memberInvitationService.AcceptAsync(seeded.Id);
    Assert.NotNull(invitation);

    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Equal(2, invitation.Version);
    Assert.Equal(new Actor(owner), invitation.CreatedBy);
    Assert.Equal(seeded.CreatedOn, invitation.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, invitation.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, invitation.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MemberInvitationStatus.Accepted, invitation.Status);

    World? world = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(world);
    Assert.True(world.IsMember(new UserId(invitee)));

    Context.User = owner;
    WorldDto? worldDto = await _worldService.ReadAsync(world.EntityId);
    Assert.NotNull(worldDto);
    MemberDto member = Assert.Single(worldDto.Members);
    Assert.Equal(new Actor(invitee), member.User);
    Assert.Equal(new Actor(owner), member.GrantedBy);
    Assert.Equal(DateTime.UtcNow, member.GrantedOn, TimeSpan.FromSeconds(10));
  }

  [Fact(DisplayName = "It should decline a member invitation.")]
  public async Task Given_Pending_When_Decline_Then_Declined()
  {
    User owner = Context.User!;
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    MemberInvitationDto? invitation = await _memberInvitationService.DeclineAsync(seeded.Id);
    Assert.NotNull(invitation);

    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Equal(2, invitation.Version);
    Assert.Equal(new Actor(owner), invitation.CreatedBy);
    Assert.Equal(seeded.CreatedOn, invitation.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, invitation.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, invitation.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MemberInvitationStatus.Declined, invitation.Status);
  }

  [Fact(DisplayName = "It should cancel a member invitation.")]
  public async Task Given_Pending_When_Cancel_Then_Cancelled()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    MemberInvitationDto? invitation = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(invitation);

    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Equal(2, invitation.Version);
    Assert.Equal(seeded.CreatedBy, invitation.CreatedBy);
    Assert.Equal(seeded.CreatedOn, invitation.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, invitation.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, invitation.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MemberInvitationStatus.Cancelled, invitation.Status);
  }

  [Fact(DisplayName = "It should return null when the invitation was not found.")]
  public async Task Given_NotFound_When_Accept_Then_NullReturned()
  {
    Assert.Null(await _memberInvitationService.AcceptAsync(Guid.Empty));
  }

  [Fact(DisplayName = "It should return null when declining a missing invitation.")]
  public async Task Given_NotFound_When_Decline_Then_NullReturned()
  {
    Assert.Null(await _memberInvitationService.DeclineAsync(Guid.Empty));
  }

  [Fact(DisplayName = "It should return null when cancelling a missing invitation.")]
  public async Task Given_NotFound_When_Cancel_Then_NullReturned()
  {
    Assert.Null(await _memberInvitationService.CancelAsync(Guid.Empty));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when accepting an invitation.")]
  public async Task Given_NotAllowed_When_Accept_Then_PermissionDeniedException()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.AcceptAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Accept", exception.Action);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when declining an invitation.")]
  public async Task Given_NotAllowed_When_Decline_Then_PermissionDeniedException()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.DeclineAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Decline", exception.Action);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when cancelling an invitation.")]
  public async Task Given_NotAllowed_When_Cancel_Then_PermissionDeniedException()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.CancelAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Cancel", exception.Action);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when accepting a cancelled invitation.")]
  public async Task Given_Cancelled_When_Accept_Then_InvalidMemberInvitationStatusException()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);
    await _memberInvitationService.CancelAsync(seeded.Id);

    Context.User = invitee;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.AcceptAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(seeded.Id, exception.MemberInvitationId);
    Assert.Equal(MemberInvitationStatus.Cancelled, exception.Status);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when declining a cancelled invitation.")]
  public async Task Given_Cancelled_When_Decline_Then_InvalidMemberInvitationStatusException()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);
    await _memberInvitationService.CancelAsync(seeded.Id);

    Context.User = invitee;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.DeclineAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(seeded.Id, exception.MemberInvitationId);
    Assert.Equal(MemberInvitationStatus.Cancelled, exception.Status);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when cancelling an accepted invitation.")]
  public async Task Given_Accepted_When_Cancel_Then_InvalidMemberInvitationStatusException()
  {
    User owner = Context.User!;
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    await _memberInvitationService.AcceptAsync(seeded.Id);

    Context.User = owner;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.CancelAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(seeded.Id, exception.MemberInvitationId);
    Assert.Equal(MemberInvitationStatus.Accepted, exception.Status);
  }

  [Fact(DisplayName = "It should not change an already accepted invitation.")]
  public async Task Given_Accepted_When_Accept_Then_Unchanged()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    MemberInvitationDto? first = await _memberInvitationService.AcceptAsync(seeded.Id);
    Assert.NotNull(first);

    MemberInvitationDto? second = await _memberInvitationService.AcceptAsync(seeded.Id);
    Assert.NotNull(second);
    Assert.Equal(first.Version, second.Version);
    Assert.Equal(MemberInvitationStatus.Accepted, second.Status);
  }

  [Fact(DisplayName = "It should not change an already declined invitation.")]
  public async Task Given_Declined_When_Decline_Then_Unchanged()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    MemberInvitationDto? first = await _memberInvitationService.DeclineAsync(seeded.Id);
    Assert.NotNull(first);

    MemberInvitationDto? second = await _memberInvitationService.DeclineAsync(seeded.Id);
    Assert.NotNull(second);
    Assert.Equal(first.Version, second.Version);
    Assert.Equal(MemberInvitationStatus.Declined, second.Status);
  }

  [Fact(DisplayName = "It should not change an already cancelled invitation.")]
  public async Task Given_Cancelled_When_Cancel_Then_Unchanged()
  {
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    MemberInvitationDto? first = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(first);

    MemberInvitationDto? second = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(second);
    Assert.Equal(first.Version, second.Version);
    Assert.Equal(MemberInvitationStatus.Cancelled, second.Status);
  }

  [Fact(DisplayName = "It should throw MemberAlreadyExistsException when inviting an accepted member.")]
  public async Task Given_AcceptedMember_When_Invite_Then_MemberAlreadyExistsException()
  {
    User owner = Context.User!;
    User invitee = new UserBuilder(Faker).Build();
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    await _memberInvitationService.AcceptAsync(seeded.Id);

    Context.User = owner;
    SetupInvitee(invitee);
    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);

    MemberAlreadyExistsException exception = await Assert.ThrowsAsync<MemberAlreadyExistsException>(
      async () => await _memberInvitationService.SendAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(invitee.Id, exception.UserId);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  private async Task ExpireInvitationAsync(Guid invitationId)
  {
    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    DateTime expiresOn = DateTime.UtcNow.AddDays(-1);
    await pokemon.Database.ExecuteSqlInterpolatedAsync(
      $@"UPDATE ""Pokemon"".""MemberInvitations"" SET ""ExpiresOn"" = {expiresOn} WHERE ""Id"" = {invitationId}");
  }

  private async Task<MemberInvitationDto> InviteUserAsync(User invitee)
  {
    SetupInvitee(invitee);
    return await _memberInvitationService.SendAsync(CreatePayload(invitee.Email!.Address));
  }

  private void SetupInvitee(User invitee)
  {
    User? owner = Context.User;
    UserClient.Setup(x => x.ReadAsync(null, invitee.Email!.Address, null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(invitee);
    UserClient.Setup(x => x.SearchAsync(It.IsAny<SearchUsersPayload>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((SearchUsersPayload payload, CancellationToken _) =>
      {
        Dictionary<Guid, User> users = [];
        if (owner is not null && payload.Ids.Contains(owner.Id))
        {
          users[owner.Id] = owner;
        }
        if (payload.Ids.Contains(invitee.Id))
        {
          users[invitee.Id] = invitee;
        }
        if (Context.User is not null && payload.Ids.Contains(Context.User.Id))
        {
          users[Context.User.Id] = Context.User;
        }
        return new SearchResults<User>(users.Values);
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
