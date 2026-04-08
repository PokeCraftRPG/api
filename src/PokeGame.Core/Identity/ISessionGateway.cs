using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface ISessionGateway
{
  Task<Session> CreateAsync(User user, CancellationToken cancellationToken = default);
  Task<Session> RenewAsync(string refreshToken, CancellationToken cancellationToken = default);
  Task<Session> SignInAsync(User user, string password, CancellationToken cancellationToken = default);
}
