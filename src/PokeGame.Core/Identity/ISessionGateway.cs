using Krakenar.Contracts.Sessions;

namespace PokeGame.Core.Identity;

public interface ISessionGateway
{
  Task<Session?> FindAsync(Guid id, CancellationToken cancellationToken = default);
  Task<Session> RenewAsync(string refreshToken, CancellationToken cancellationToken = default);
}
