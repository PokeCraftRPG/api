using Krakenar.Client;
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

  public async Task<User?> FindAsync(string emailAddress, CancellationToken cancellationToken)
  {
    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    return await _userClient.ReadAsync(id: null, emailAddress, customIdentifier: null, context);
  }

  public async Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    if (!ids.Any())
    {
      return new List<User>().AsReadOnly();
    }

    SearchUsersPayload payload = new();
    payload.Ids.AddRange(ids);

    RequestContext context = new RequestContextBuilder(cancellationToken).Build();
    SearchResults<User> results = await _userClient.SearchAsync(payload, context);

    return results.Items.AsReadOnly();
  }
}
