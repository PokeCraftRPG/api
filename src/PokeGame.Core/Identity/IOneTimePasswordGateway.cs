using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public interface IOneTimePasswordGateway
{
  Task<OneTimePassword> CreateMultiFactorAuthenticationAsync(User user, CancellationToken cancellationToken = default);
  Task<User> ValidateMultiFactorAuthenticationAsync(OneTimePasswordValidation validation, CancellationToken cancellationToken = default);
}
