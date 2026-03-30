using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;
using PokeGame.Core.Accounts.Models;

namespace PokeGame.Core.Identity;

public interface IOneTimePasswordGateway
{
  Task<OneTimePassword> CreateMultiFactorAuthenticationAsync(User user, CancellationToken cancellationToken = default);
  Task<User> ValidateMultiFactorAuthenticationAsync(OneTimePasswordValidation oneTimePassword, CancellationToken cancellationToken = default);
}
