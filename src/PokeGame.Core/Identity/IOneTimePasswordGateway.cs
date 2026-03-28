using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface IOneTimePasswordGateway
{
  Task<OneTimePassword> CreateAsync(User user, OneTimePasswordOptions options, CancellationToken cancellationToken = default);
}
