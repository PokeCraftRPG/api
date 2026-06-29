using Krakenar.Client.Users;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class UserGateway : IUserGateway
{
  private readonly IUserClient _userClient;

  public UserGateway(IUserClient userClient)
  {
    _userClient = userClient;
  }

  public async Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    SearchUsersPayload payload = new();
    payload.Ids.AddRange(ids);

    SearchResults<User> results = await _userClient.SearchAsync(payload, cancellationToken);
    return results.Items;
  }
}
