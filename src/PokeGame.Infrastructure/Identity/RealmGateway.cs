using Krakenar.Client;
using Krakenar.Contracts.Realms;
using PokeGame.Core.Identity;

namespace PokeGame.Infrastructure.Identity;

internal class RealmGateway : IRealmGateway
{
  private readonly IKrakenarSettings _krakenar;
  private readonly IRealmService _realmService;

  public RealmGateway(IKrakenarSettings krakenar, IRealmService realmService)
  {
    _krakenar = krakenar;
    _realmService = realmService;
  }

  public async Task<Realm> FindAsync(CancellationToken cancellationToken)
  {
    return await _realmService.ReadAsync(id: null, _krakenar.Realm, cancellationToken) ?? throw new InvalidOperationException($"The realm 'Slug={_krakenar.Realm}' was not found.");
  }
}
