using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public interface ITokenGateway
{
  Task<TokenResponse> GetResponseAsync(Session session, CancellationToken cancellationToken = default);
  Task<User> ValidateAccessAsync(string token, CancellationToken cancellationToken = default);
}
