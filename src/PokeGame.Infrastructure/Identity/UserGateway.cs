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

  public async Task<User> AuthenticateAsync(string uniqueName, string password, CancellationToken cancellationToken)
  {
    AuthenticateUserPayload payload = new(uniqueName, password);
    return await _userClient.AuthenticateAsync(payload, cancellationToken);
  }

  public async Task<User?> FindAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _userClient.ReadAsync(id, uniqueName: null, customIdentifier: null, cancellationToken);
  }
  public async Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    if (!ids.Any())
    {
      return [];
    }
    SearchUsersPayload payload = new();
    payload.Ids.AddRange(ids);
    SearchResults<User> results = await _userClient.SearchAsync(payload, cancellationToken);
    return results.Items;
  }
}
