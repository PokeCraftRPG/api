using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Membership;

[Trait(Traits.Category, Categories.Integration)]
public class MemberIntegrationTests : IntegrationTests
{
  private readonly ICacheService _cacheService;
  private readonly IMembershipService _membershipService;
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  public MemberIntegrationTests()
  {
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _membershipService = ServiceProvider.GetRequiredService<IMembershipService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  [Fact(DisplayName = "It should revoke a world membership.")]
  public async Task Given_Member_When_Revoke_Then_Revoked()
  {
    User owner = Context.User!;
    User member = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(member);
    SetupUsers(member);

    WorldDto? world = await _membershipService.RevokeAsync(Context.WorldId.EntityId, new RevokeMembershipPayload { UserId = member.Id });
    Assert.NotNull(world);
    AssertOwnerOnly(world, owner);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.False(loaded.IsMember(new UserId(member)));
  }

  [Fact(DisplayName = "It should keep other members when revoking a membership.")]
  public async Task Given_OtherMembers_When_Revoke_Then_OtherMembersKept()
  {
    User owner = Context.User!;
    User revoked = KrakenarFactory.Instance.NewUser(Faker);
    User remaining = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(revoked, remaining);
    SetupUsers(remaining);

    WorldDto? world = await _membershipService.RevokeAsync(Context.WorldId.EntityId, new RevokeMembershipPayload { UserId = revoked.Id });
    Assert.NotNull(world);
    Assert.Equal(2, world.Members.Count);
    Assert.Contains(world.Members, member => member.User.Equals(new Actor(owner)));
    Assert.Contains(world.Members, member => member.User.Equals(new Actor(remaining)));

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.False(loaded.IsMember(new UserId(revoked)));
    Assert.True(loaded.IsMember(new UserId(remaining)));
  }

  [Fact(DisplayName = "It should not change the world when the user is not a member.")]
  public async Task Given_NotMember_When_Revoke_Then_Unchanged()
  {
    User owner = Context.User!;

    WorldDto? world = await _membershipService.RevokeAsync(Context.WorldId.EntityId, new RevokeMembershipPayload { UserId = Guid.Empty });
    Assert.NotNull(world);
    AssertOwnerOnly(world, owner);
  }

  [Fact(DisplayName = "It should return null when revoking a membership from a missing world.")]
  public async Task Given_MissingWorld_When_Revoke_Then_Null()
  {
    WorldDto? world = await _membershipService.RevokeAsync(Guid.NewGuid(), new RevokeMembershipPayload { UserId = Guid.Empty });
    Assert.Null(world);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when revoking a membership.")]
  public async Task Given_NotAllowed_When_Revoke_Then_PermissionDeniedException()
  {
    User member = await GrantMembershipAsync();
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _membershipService.RevokeAsync(Context.WorldId.EntityId, new RevokeMembershipPayload { UserId = member.Id }));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("RevokeMember", exception.Action);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should leave a world membership.")]
  public async Task Given_Member_When_Leave_Then_Left()
  {
    User owner = Context.User!;
    User member = await GrantMembershipAsync();
    Context.User = member;

    bool left = await _membershipService.LeaveAsync(Context.WorldId.EntityId);
    Assert.True(left);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.False(loaded.IsMember(new UserId(member)));

    Context.User = owner;
    SetupUsers(member);
    WorldDto? world = await _worldService.ReadAsync(loaded.EntityId);
    Assert.NotNull(world);
    AssertOwnerOnly(world, owner);
  }

  [Fact(DisplayName = "It should keep other members when leaving a membership.")]
  public async Task Given_OtherMembers_When_Leave_Then_OtherMembersKept()
  {
    User owner = Context.User!;
    User leaving = KrakenarFactory.Instance.NewUser(Faker);
    User remaining = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(leaving, remaining);
    Context.User = leaving;

    bool left = await _membershipService.LeaveAsync(Context.WorldId.EntityId);
    Assert.True(left);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.False(loaded.IsMember(new UserId(leaving)));
    Assert.True(loaded.IsMember(new UserId(remaining)));

    Context.User = owner;
    SetupUsers(remaining, leaving);
    WorldDto? world = await _worldService.ReadAsync(loaded.EntityId);
    Assert.NotNull(world);
    Assert.Equal(2, world.Members.Count);
    Assert.Contains(world.Members, member => member.User.Equals(new Actor(owner)));
    Assert.Contains(world.Members, member => member.User.Equals(new Actor(remaining)));
  }

  [Fact(DisplayName = "It should return false when leaving a missing world.")]
  public async Task Given_MissingWorld_When_Leave_Then_False()
  {
    User member = await GrantMembershipAsync();
    Context.User = member;

    bool left = await _membershipService.LeaveAsync(Guid.NewGuid());
    Assert.False(left);
  }

  [Fact(DisplayName = "It should throw OwnerCannotLeaveWorldException when the owner leaves a membership.")]
  public async Task Given_Owner_When_Leave_Then_OwnerCannotLeaveWorldException()
  {
    User owner = Context.User!;

    OwnerCannotLeaveWorldException exception = await Assert.ThrowsAsync<OwnerCannotLeaveWorldException>(
      async () => await _membershipService.LeaveAsync(Context.WorldId.EntityId));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(owner.Id, exception.OwnerId);
  }

  [Fact(DisplayName = "It should throw WorldOwnershipCannotBeRevokedException when revoking the owner.")]
  public async Task Given_Owner_When_Revoke_Then_WorldOwnershipCannotBeRevokedException()
  {
    User owner = Context.User!;

    WorldOwnershipCannotBeRevokedException exception = await Assert.ThrowsAsync<WorldOwnershipCannotBeRevokedException>(
      async () => await _membershipService.RevokeAsync(Context.WorldId.EntityId, new RevokeMembershipPayload { UserId = owner.Id }));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(owner.Id, exception.OwnerId);
  }

  [Fact(DisplayName = "It should transfer world ownership to a member.")]
  public async Task Given_Member_When_Transfer_Then_Transferred()
  {
    User owner = Context.User!;
    User member = await GrantMembershipAsync();
    SetupUsers(member);

    WorldDto? world = await _membershipService.TransferOwnershipAsync(Context.WorldId.EntityId, new TransferOwnershipPayload { UserId = member.Id });
    Assert.NotNull(world);
    Assert.Equal(new Actor(member), world.Owner);

    MemberDto formerOwner = Assert.Single(world.Members);
    Assert.Equal(new Actor(owner), formerOwner.User);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.Equal(new UserId(member), loaded.OwnerId);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.True(loaded.IsMember(new UserId(member)));
  }

  [Fact(DisplayName = "It should keep other members when transferring ownership.")]
  public async Task Given_OtherMembers_When_Transfer_Then_OtherMembersKept()
  {
    User owner = Context.User!;
    User successor = KrakenarFactory.Instance.NewUser(Faker);
    User remaining = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(successor, remaining);
    SetupUsers(successor, remaining);

    WorldDto? world = await _membershipService.TransferOwnershipAsync(Context.WorldId.EntityId, new TransferOwnershipPayload { UserId = successor.Id });
    Assert.NotNull(world);
    Assert.Equal(new Actor(successor), world.Owner);

    MemberDto formerOwner = Assert.Single(world.Members);
    Assert.Equal(new Actor(owner), formerOwner.User);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.Equal(new UserId(successor), loaded.OwnerId);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.True(loaded.IsMember(new UserId(remaining)));
    Assert.True(loaded.IsMember(new UserId(successor)));
  }

  [Fact(DisplayName = "It should not change the world when transferring ownership to the owner.")]
  public async Task Given_Owner_When_Transfer_Then_Unchanged()
  {
    User owner = Context.User!;
    User member = await GrantMembershipAsync();
    SetupUsers(member);

    WorldDto? world = await _membershipService.TransferOwnershipAsync(Context.WorldId.EntityId, new TransferOwnershipPayload { UserId = owner.Id });
    Assert.NotNull(world);
    Assert.Equal(new Actor(owner), world.Owner);
    Assert.Equal(2, world.Members.Count);
    Assert.Contains(world.Members, m => m.User.Equals(new Actor(owner)));
    Assert.Contains(world.Members, m => m.User.Equals(new Actor(member)));

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.Equal(new UserId(owner), loaded.OwnerId);
    Assert.True(loaded.IsMember(new UserId(owner)));
    Assert.True(loaded.IsMember(new UserId(member)));
  }

  [Fact(DisplayName = "It should return null when transferring ownership of a missing world.")]
  public async Task Given_MissingWorld_When_Transfer_Then_Null()
  {
    WorldDto? world = await _membershipService.TransferOwnershipAsync(Guid.NewGuid(), new TransferOwnershipPayload { UserId = Guid.Empty });
    Assert.Null(world);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when transferring ownership.")]
  public async Task Given_NotAllowed_When_Transfer_Then_PermissionDeniedException()
  {
    User member = await GrantMembershipAsync();
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _membershipService.TransferOwnershipAsync(Context.WorldId.EntityId, new TransferOwnershipPayload { UserId = member.Id }));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("TransferOwnership", exception.Action);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw UserIsNotMemberException when transferring ownership to a non-member.")]
  public async Task Given_NotMember_When_Transfer_Then_UserIsNotMemberException()
  {
    User user = KrakenarFactory.Instance.NewUser(Faker);

    UserIsNotMemberException exception = await Assert.ThrowsAsync<UserIsNotMemberException>(
      async () => await _membershipService.TransferOwnershipAsync(Context.WorldId.EntityId, new TransferOwnershipPayload { UserId = user.Id }));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(user.Id, exception.UserId);
  }

  private static void AssertOwnerOnly(WorldDto world, User owner)
  {
    MemberDto member = Assert.Single(world.Members);
    Assert.Equal(new Actor(owner), member.User);
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
}
