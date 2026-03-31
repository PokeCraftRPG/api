using Moq;

namespace PokeGame.Core.Membership.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadMembershipInvitationQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IMembershipInvitationQuerier> _membershipinvitationQuerier = new();

  private readonly ReadMembershipInvitationQueryHandler _handler;

  public ReadMembershipInvitationQueryHandlerTests()
  {
    _handler = new(_membershipinvitationQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no membershipinvitation was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadMembershipInvitationQuery query = new(Guid.Empty);
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }
}
