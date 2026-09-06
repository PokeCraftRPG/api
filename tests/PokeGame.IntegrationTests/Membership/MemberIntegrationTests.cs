using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
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

  public MemberIntegrationTests()
  {
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _membershipService = ServiceProvider.GetRequiredService<IMembershipService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  [Fact(DisplayName = "It should revoke a world membership.")]
  public async Task Given_Member_When_Revoke_Then_Revoked()
  {
    User member = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(member);
    SetupUsers(member);

    WorldDto? world = await _membershipService.RevokeAsync(member.Id);
    Assert.NotNull(world);
    Assert.Empty(world.Members);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.False(loaded.IsMember(new UserId(member)));
  }

  [Fact(DisplayName = "It should keep other members when revoking a membership.")]
  public async Task Given_OtherMembers_When_Revoke_Then_OtherMembersKept()
  {
    User revoked = KrakenarFactory.Instance.NewUser(Faker);
    User remaining = KrakenarFactory.Instance.NewUser(Faker);
    await GrantMembershipAsync(revoked, remaining);
    SetupUsers(remaining);

    WorldDto? world = await _membershipService.RevokeAsync(revoked.Id);
    Assert.NotNull(world);

    MemberDto member = Assert.Single(world.Members);
    Assert.Equal(new Actor(remaining), member.User);

    World? loaded = await _worldRepository.LoadAsync(Context.WorldId);
    Assert.NotNull(loaded);
    Assert.False(loaded.IsMember(new UserId(revoked)));
    Assert.True(loaded.IsMember(new UserId(remaining)));
  }

  [Fact(DisplayName = "It should not change the world when the user is not a member.")]
  public async Task Given_NotMember_When_Revoke_Then_Unchanged()
  {
    WorldDto? world = await _membershipService.RevokeAsync(Guid.Empty);
    Assert.NotNull(world);
    Assert.Empty(world.Members);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when revoking a membership.")]
  public async Task Given_NotAllowed_When_Revoke_Then_PermissionDeniedException()
  {
    User member = await GrantMembershipAsync();
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _membershipService.RevokeAsync(member.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("RevokeMember", exception.Action);
    Assert.Equal(Context.World!.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
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
