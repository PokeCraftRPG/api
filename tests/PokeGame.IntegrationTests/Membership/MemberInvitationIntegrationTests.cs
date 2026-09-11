using FluentValidation;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Caching;
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
  private readonly ICacheService _cacheService;
  private readonly IMemberInvitationRepository _memberInvitationRepository;
  private readonly IMemberInvitationService _memberInvitationService;
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  public MemberInvitationIntegrationTests()
  {
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _memberInvitationRepository = ServiceProvider.GetRequiredService<IMemberInvitationRepository>();
    _memberInvitationService = ServiceProvider.GetRequiredService<IMemberInvitationService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  [Fact(DisplayName = "It should invite a member by email address.")]
  public async Task Given_UnknownEmail_When_Invite_Then_Created()
  {
    User member = await GrantMembershipAsync();
    SendMemberInvitationPayload payload = CreatePayload();

    MemberInvitationDto invitation = await SendInvitationAsync(payload);

    AssertInvitation(invitation, payload);
    Assert.Equal(ActorType.User, invitation.Invitee.Type);
    Assert.Equal(payload.EmailAddress, invitation.Invitee.EmailAddress);
    Assert.Equal(payload.EmailAddress, invitation.Invitee.DisplayName);
    AssertWorldMembers(invitation, member);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.Is<MemberInvitation>(i => i.EntityId == invitation.Id),
      payload.Locale,
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should invite an existing user.")]
  public async Task Given_ExistingUser_When_Invite_Then_Created()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    SetupInvitee(invitee);

    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);

    MemberInvitationDto invitation = await SendInvitationAsync(payload);

    AssertInvitation(invitation, payload);
    Assert.Equal(new Actor(invitee), invitation.Invitee);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.Is<MemberInvitation>(i => i.EntityId == invitation.Id
        && i.UserId == new UserId(invitee)
        && i.EmailAddress.Value == EmailAddress.Format(invitee.Email.Address)),
      payload.Locale,
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should invite the same email in another world.")]
  public async Task Given_OtherWorld_When_Invite_Then_Created()
  {
    SendMemberInvitationPayload payload = CreatePayload();
    MemberInvitationDto first = await SendInvitationAsync(payload);

    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("other-world").Build();
    await _worldRepository.SaveAsync(world);
    Context.World = world;

    MemberInvitationDto second = await SendInvitationAsync(payload, world.EntityId);
    Assert.NotEqual(first.Id, second.Id);
    Assert.Equal(world.EntityId, second.World.Id);
  }

  [Fact(DisplayName = "It should claim unassigned pending invitations for an email address.")]
  public async Task Given_UnassignedPending_When_Claim_Then_Assigned()
  {
    string email = "claim.me@example.com";
    MemberInvitationDto seeded = await SendInvitationAsync(CreatePayload(email));

    User claimer = KrakenarFactory.Instance.NewUser(Faker);
    UserId userId = new(claimer);
    await _memberInvitationService.ClaimAsync(userId, new EmailAddress(email));

    MemberInvitation? loaded = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(seeded.Id));
    Assert.NotNull(loaded);
    Assert.Equal(userId, loaded.UserId);
    Assert.Equal(EmailAddress.Format(email), loaded.EmailAddress.Value);
    Assert.Equal(MemberInvitationStatus.Pending, loaded.Status);

    SetupInvitee(claimer);
    MemberInvitationDto? dto = await _memberInvitationService.ReadAsync(seeded.Id);
    Assert.NotNull(dto);
    Assert.Equal(new Actor(claimer), dto.Invitee);
  }

  [Fact(DisplayName = "It should claim invitations across worlds for the same email.")]
  public async Task Given_MultipleWorlds_When_Claim_Then_AllAssigned()
  {
    string email = "multi.world@example.com";
    MemberInvitationDto first = await SendInvitationAsync(CreatePayload(email));

    World otherWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("claim-other-world").Build();
    await _worldRepository.SaveAsync(otherWorld);
    Context.World = otherWorld;
    MemberInvitationDto second = await SendInvitationAsync(CreatePayload(email), otherWorld.EntityId);

    User claimer = KrakenarFactory.Instance.NewUser(Faker);
    UserId userId = new(claimer);
    await _memberInvitationService.ClaimAsync(userId, new EmailAddress(email));

    MemberInvitation? loadedFirst = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(first.Id));
    Assert.NotNull(loadedFirst);
    Assert.Equal(userId, loadedFirst.UserId);

    MemberInvitation? loadedSecond = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(second.Id));
    Assert.NotNull(loadedSecond);
    Assert.Equal(userId, loadedSecond.UserId);
  }

  [Fact(DisplayName = "It should not claim cancelled, expired or already assigned invitations.")]
  public async Task Given_IneligibleInvitations_When_Claim_Then_Unchanged()
  {
    string email = "skip.me@example.com";

    MemberInvitationDto cancelled = await SendInvitationAsync(CreatePayload(email));
    await _memberInvitationService.CancelAsync(cancelled.Id);

    MemberInvitationDto expired = await SendInvitationAsync(CreatePayload(email));
    await ExpireInvitationAsync(expired.Id);

    MemberInvitationDto pending = await SendInvitationAsync(CreatePayload(email));

    User existingInvitee = KrakenarFactory.Instance.NewUser(Faker);
    SetupInvitee(existingInvitee);
    MemberInvitationDto assigned = await SendInvitationAsync(CreatePayload(existingInvitee.Email!.Address));

    User claimer = KrakenarFactory.Instance.NewUser(Faker);
    UserId userId = new(claimer);
    await _memberInvitationService.ClaimAsync(userId, new EmailAddress(email));
    await _memberInvitationService.ClaimAsync(userId, new EmailAddress(existingInvitee.Email.Address));

    MemberInvitation? loadedPending = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(pending.Id));
    Assert.NotNull(loadedPending);
    Assert.Equal(userId, loadedPending.UserId);

    MemberInvitation? loadedCancelled = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(cancelled.Id));
    Assert.NotNull(loadedCancelled);
    Assert.Null(loadedCancelled.UserId);

    MemberInvitation? loadedExpired = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(expired.Id));
    Assert.NotNull(loadedExpired);
    Assert.Null(loadedExpired.UserId);

    MemberInvitation? loadedAssigned = await _memberInvitationRepository.LoadAsync(new MemberInvitationId(assigned.Id));
    Assert.NotNull(loadedAssigned);
    Assert.Equal(new UserId(existingInvitee), loadedAssigned.UserId);
  }

  [Fact(DisplayName = "It should do nothing when there is no invitation to claim.")]
  public async Task Given_NoInvitation_When_Claim_Then_NoOp()
  {
    User claimer = KrakenarFactory.Instance.NewUser(Faker);
    await _memberInvitationService.ClaimAsync(new UserId(claimer), new EmailAddress("nobody@example.com"));
  }

  [Fact(DisplayName = "It should throw UserIsAlreadyMemberException when the user is already a member.")]
  public async Task Given_ExistingMember_When_Invite_Then_UserIsAlreadyMemberException()
  {
    User member = Context.User!;
    SetupInvitee(member);

    SendMemberInvitationPayload payload = CreatePayload(member.Email!.Address);

    UserIsAlreadyMemberException exception = await Assert.ThrowsAsync<UserIsAlreadyMemberException>(
      async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(member.Id, exception.Data["UserId"]);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should return null when the world does not exist.")]
  public async Task Given_MissingWorld_When_Invite_Then_NullReturned()
  {
    Guid missingWorldId = Guid.NewGuid();
    SendMemberInvitationPayload payload = CreatePayload();

    MemberInvitationDto? invitation = await _memberInvitationService.SendAsync(missingWorldId, payload);
    Assert.Null(invitation);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should throw MemberInvitationAlreadyPendingException when the email is already invited.")]
  public async Task Given_PendingEmail_When_Invite_Then_MemberInvitationAlreadyPendingException()
  {
    SendMemberInvitationPayload payload = CreatePayload();
    MemberInvitationDto existing = await SendInvitationAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(existing.Id, exception.Data["InvitationId"]);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should throw MemberInvitationAlreadyPendingException when the user is already invited.")]
  public async Task Given_PendingUser_When_Invite_Then_MemberInvitationAlreadyPendingException()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    SetupInvitee(invitee);

    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);
    MemberInvitationDto existing = await SendInvitationAsync(payload);

    MemberInvitationAlreadyPendingException exception = await Assert.ThrowsAsync<MemberInvitationAlreadyPendingException>(
      async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(existing.Id, exception.Data["InvitationId"]);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Invite_Then_ValidationException()
  {
    SendMemberInvitationPayload payload = new();

    await Assert.ThrowsAsync<ValidationException>(async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when inviting a member.")]
  public async Task Given_NotAllowed_When_Invite_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    SendMemberInvitationPayload payload = CreatePayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("InviteMember", exception.Data["Action"]);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact(DisplayName = "It should allow the world owner to read a member invitation by ID.")]
  public async Task Given_WorldOwner_When_Read_Then_Read()
  {
    User member = await GrantMembershipAsync();
    MemberInvitationDto seeded = await SendInvitationAsync();

    MemberInvitationDto? invitation = await _memberInvitationService.ReadAsync(seeded.Id);
    Assert.NotNull(invitation);
    Assert.Equal(seeded.Id, invitation.Id);
    AssertWorldMembers(invitation, member);
  }

  [Fact(DisplayName = "It should allow the invitee to read a member invitation by ID.")]
  public async Task Given_Invitee_When_Read_Then_Read()
  {
    User member = await GrantMembershipAsync();
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee, member);

    Context.User = invitee;
    MemberInvitationDto? invitation = await _memberInvitationService.ReadAsync(seeded.Id);
    Assert.NotNull(invitation);
    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Empty(invitation.World.Members);
  }

  [Fact(DisplayName = "It should return null when the current user cannot read the invitation.")]
  public async Task Given_NotAllowed_When_Read_Then_NullReturned()
  {
    MemberInvitationDto seeded = await SendInvitationAsync();
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    Assert.Null(await _memberInvitationService.ReadAsync(seeded.Id));
  }

  [Fact(DisplayName = "It should return null when no invitation was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Assert.Null(await _memberInvitationService.ReadAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should return empty world search results.")]
  public async Task Given_NoMatch_When_SearchWorld_Then_EmptyResults()
  {
    await SendInvitationAsync();
    World otherWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("other-world").Build();
    await _worldRepository.SaveAsync(otherWorld);

    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<MemberInvitationDto>? results = await _memberInvitationService.SearchWorldAsync(otherWorld.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when searching invitations of a missing world.")]
  public async Task Given_MissingWorld_When_SearchWorld_Then_NullReturned()
  {
    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    Assert.Null(await _memberInvitationService.SearchWorldAsync(Guid.NewGuid(), payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when searching world invitations.")]
  public async Task Given_NotAllowed_When_SearchWorld_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.SearchWorldAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("ViewInvitations", exception.Data["Action"]);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should return the correct world search results.")]
  public async Task Given_Matches_When_SearchWorld_Then_Results()
  {
    MemberInvitationDto ash = await SendInvitationAsync(CreatePayload("ash.ketchum@example.com"));
    MemberInvitationDto misty = await SendInvitationAsync(CreatePayload("misty.waterflower@example.com"));
    await SendInvitationAsync(CreatePayload("brock.harrison@example.com"));
    await _memberInvitationService.CancelAsync(misty.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Ids.AddRange([ash.Id, misty.Id]);
    payload.Sort.Add(new SortOption<MemberInvitationSort>(MemberInvitationSort.UpdatedOn, SortDirection.Descending));

    SearchResults<MemberInvitationDto>? results = await _memberInvitationService.SearchWorldAsync(Context.WorldId.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(2, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(ash.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should filter world search results by status.")]
  public async Task Given_StatusFilter_When_SearchWorld_Then_Results()
  {
    await SendInvitationAsync(CreatePayload("pending@example.com"));
    MemberInvitationDto cancelled = await SendInvitationAsync(CreatePayload("cancelled@example.com"));
    await _memberInvitationService.CancelAsync(cancelled.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      Status = MemberInvitationStatus.Cancelled,
      Limit = 10
    };

    SearchResults<MemberInvitationDto>? results = await _memberInvitationService.SearchWorldAsync(Context.WorldId.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(cancelled.Id, invitation.Id);
    Assert.Equal(MemberInvitationStatus.Cancelled, invitation.Status);
  }

  [Theory(DisplayName = "It should filter world search results by expiration.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_ExpiredFilter_When_SearchWorld_Then_Results(bool isExpired)
  {
    MemberInvitationDto active = await SendInvitationAsync(CreatePayload("active@example.com"));
    MemberInvitationDto expired = await SendInvitationAsync(CreatePayload("expired@example.com"));
    await ExpireInvitationAsync(expired.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      IsExpired = isExpired,
      Limit = 10
    };

    SearchResults<MemberInvitationDto>? results = await _memberInvitationService.SearchWorldAsync(Context.WorldId.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(isExpired ? expired.Id : active.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should return empty received search results.")]
  public async Task Given_NoMatch_When_SearchReceived_Then_EmptyResults()
  {
    await SendInvitationAsync();

    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchReceivedAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return the invitee's received invitations.")]
  public async Task Given_Invitee_When_SearchReceived_Then_Results()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);
    await SendInvitationAsync(CreatePayload("other@example.com"));

    Context.User = invitee;
    SearchMemberInvitationsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchReceivedAsync(payload);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(seeded.Id, invitation.Id);
  }

  [Fact(DisplayName = "It should filter received search results by status.")]
  public async Task Given_StatusFilter_When_SearchReceived_Then_Results()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    await _memberInvitationService.DeclineAsync(seeded.Id);

    SearchMemberInvitationsPayload payload = new()
    {
      Status = MemberInvitationStatus.Declined,
      Limit = 10
    };

    SearchResults<MemberInvitationDto> results = await _memberInvitationService.SearchReceivedAsync(payload);
    Assert.Equal(1, results.Total);

    MemberInvitationDto invitation = Assert.Single(results.Items);
    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Equal(MemberInvitationStatus.Declined, invitation.Status);
  }

  [Fact(DisplayName = "It should accept a member invitation.")]
  public async Task Given_Pending_When_Accept_Then_Accepted()
  {
    User owner = Context.User!;
    User member = await GrantMembershipAsync();
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee, member);

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

    MemberDto current = Assert.Single(invitation.World.Members);
    Assert.Equal(new Actor(invitee), current.User);
    Assert.Equal(new Actor(owner), current.GrantedBy);
    Assert.Equal(DateTime.UtcNow, current.GrantedOn, TimeSpan.FromSeconds(10));

    World? world = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(world);
    Assert.True(world.IsMember(new UserId(invitee)));

    Context.User = owner;
    WorldDto? worldDto = await _worldService.ReadAsync(world.EntityId);
    Assert.NotNull(worldDto);
    Assert.Contains(worldDto.Members, member => member.User.Equals(new Actor(invitee)));
  }

  [Fact(DisplayName = "It should decline a member invitation.")]
  public async Task Given_Pending_When_Decline_Then_Declined()
  {
    User owner = Context.User!;
    User member = await GrantMembershipAsync();
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee, member);

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
    Assert.Empty(invitation.World.Members);
  }

  [Fact(DisplayName = "It should cancel a member invitation.")]
  public async Task Given_Pending_When_Cancel_Then_Cancelled()
  {
    User member = await GrantMembershipAsync();
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee, member);

    MemberInvitationDto? invitation = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(invitation);

    Assert.Equal(seeded.Id, invitation.Id);
    Assert.Equal(2, invitation.Version);
    Assert.Equal(seeded.CreatedBy, invitation.CreatedBy);
    Assert.Equal(seeded.CreatedOn, invitation.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, invitation.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, invitation.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(MemberInvitationStatus.Cancelled, invitation.Status);
    AssertWorldMembers(invitation, member);
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
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.AcceptAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Accept", exception.Data["Action"]);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when declining an invitation.")]
  public async Task Given_NotAllowed_When_Decline_Then_PermissionDeniedException()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.DeclineAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Decline", exception.Data["Action"]);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when cancelling an invitation.")]
  public async Task Given_NotAllowed_When_Cancel_Then_PermissionDeniedException()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _memberInvitationService.CancelAsync(seeded.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Cancel", exception.Data["Action"]);
    Assert.Equal(new Entity(MemberInvitation.EntityKind, seeded.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when accepting a cancelled invitation.")]
  public async Task Given_Cancelled_When_Accept_Then_InvalidMemberInvitationStatusException()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);
    await _memberInvitationService.CancelAsync(seeded.Id);

    Context.User = invitee;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.AcceptAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(seeded.Id, exception.Data["InvitationId"]);
    Assert.Equal(MemberInvitationStatus.Cancelled, exception.Data["Status"]);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when declining a cancelled invitation.")]
  public async Task Given_Cancelled_When_Decline_Then_InvalidMemberInvitationStatusException()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);
    await _memberInvitationService.CancelAsync(seeded.Id);

    Context.User = invitee;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.DeclineAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(seeded.Id, exception.Data["InvitationId"]);
    Assert.Equal(MemberInvitationStatus.Cancelled, exception.Data["Status"]);
  }

  [Fact(DisplayName = "It should throw InvalidMemberInvitationStatusException when cancelling an accepted invitation.")]
  public async Task Given_Accepted_When_Cancel_Then_InvalidMemberInvitationStatusException()
  {
    User owner = Context.User!;
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    await _memberInvitationService.AcceptAsync(seeded.Id);

    Context.User = owner;
    InvalidMemberInvitationStatusException exception = await Assert.ThrowsAsync<InvalidMemberInvitationStatusException>(
      async () => await _memberInvitationService.CancelAsync(seeded.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(seeded.Id, exception.Data["InvitationId"]);
    Assert.Equal(MemberInvitationStatus.Accepted, exception.Data["Status"]);
  }

  [Fact(DisplayName = "It should not change an already accepted invitation.")]
  public async Task Given_Accepted_When_Accept_Then_Unchanged()
  {
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
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
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
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
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    MemberInvitationDto? first = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(first);

    MemberInvitationDto? second = await _memberInvitationService.CancelAsync(seeded.Id);
    Assert.NotNull(second);
    Assert.Equal(first.Version, second.Version);
    Assert.Equal(MemberInvitationStatus.Cancelled, second.Status);
  }

  [Fact(DisplayName = "It should throw UserIsAlreadyMemberException when inviting an accepted member.")]
  public async Task Given_AcceptedMember_When_Invite_Then_UserIsAlreadyMemberException()
  {
    User owner = Context.User!;
    User invitee = KrakenarFactory.Instance.NewUser(Faker);
    MemberInvitationDto seeded = await InviteUserAsync(invitee);

    Context.User = invitee;
    await _memberInvitationService.AcceptAsync(seeded.Id);

    Context.User = owner;
    SetupInvitee(invitee);
    SendMemberInvitationPayload payload = CreatePayload(invitee.Email!.Address);

    UserIsAlreadyMemberException exception = await Assert.ThrowsAsync<UserIsAlreadyMemberException>(
      async () => await _memberInvitationService.SendAsync(Context.WorldId.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(invitee.Id, exception.Data["UserId"]);
    MessageGateway.Verify(x => x.SendMemberInvitationAsync(
      It.IsAny<MemberInvitation>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  private async Task<User> GrantMembershipAsync(params User[] members)
  {
    _cacheService.Realm = KrakenarFactory.Instance.Realm;

    if (members.Length == 0)
    {
      members = [KrakenarFactory.Instance.NewUser(Faker)];
    }

    foreach (User member in members)
    {
      Context.World!.GrantMembership(new UserId(member.Id, _cacheService.Realm!.Id), Context.ActorId);
    }
    await _worldRepository.SaveAsync(Context.World!);

    SetupUsers(members);
    return members[0];
  }

  private void SetupUsers(params User[] users)
  {
    UserClient.Setup(x => x.SearchAsync(It.IsAny<SearchUsersPayload>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((SearchUsersPayload payload, CancellationToken _) =>
      {
        Dictionary<Guid, User> found = [];
        if (Context.User is not null && payload.Ids.Contains(Context.User.Id))
        {
          found[Context.User.Id] = Context.User;
        }
        foreach (User user in users)
        {
          if (payload.Ids.Contains(user.Id))
          {
            found[user.Id] = user;
          }
        }
        return new SearchResults<User>(found.Values);
      });
  }

  private void AssertWorldMembers(MemberInvitationDto invitation, User member)
  {
    Assert.Equal(2, invitation.World.Members.Count);
    Assert.Contains(invitation.World.Members, m => m.User.Equals(Actor));
    MemberDto granted = Assert.Single(invitation.World.Members, m => m.User.Equals(new Actor(member)));
    Assert.Equal(Actor, granted.GrantedBy);
    Assert.Equal(DateTime.UtcNow, granted.GrantedOn, TimeSpan.FromSeconds(10));
  }

  private async Task ExpireInvitationAsync(Guid invitationId)
  {
    PokemonContext pokemon = ServiceProvider.GetRequiredService<PokemonContext>();
    DateTime expiresOn = DateTime.UtcNow.AddDays(-1);
    await pokemon.Database.ExecuteSqlInterpolatedAsync(
      $@"UPDATE ""Pokemon"".""MemberInvitations"" SET ""ExpiresOn"" = {expiresOn} WHERE ""Id"" = {invitationId}");
  }

  private async Task<MemberInvitationDto> InviteUserAsync(User invitee, params User[] knownUsers)
  {
    SetupInvitee(invitee, knownUsers);
    return await SendInvitationAsync(CreatePayload(invitee.Email!.Address));
  }

  private async Task<MemberInvitationDto> SendInvitationAsync(SendMemberInvitationPayload? payload = null, Guid? worldId = null)
  {
    MemberInvitationDto? invitation = await _memberInvitationService.SendAsync(worldId ?? Context.WorldId.EntityId, payload ?? CreatePayload());
    Assert.NotNull(invitation);
    return invitation;
  }

  private void SetupInvitee(User invitee, params User[] knownUsers)
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
        if (Context.User is not null && payload.Ids.Contains(Context.User.Id))
        {
          users[Context.User.Id] = Context.User;
        }
        foreach (User user in knownUsers.Prepend(invitee))
        {
          if (payload.Ids.Contains(user.Id))
          {
            users[user.Id] = user;
          }
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
