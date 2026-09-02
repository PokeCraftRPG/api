using Krakenar.Contracts.Realms;

namespace PokeGame.Core.Identity;

public interface IRealmGateway
{
  Task<Realm> FindAsync(CancellationToken cancellationToken = default);
}
