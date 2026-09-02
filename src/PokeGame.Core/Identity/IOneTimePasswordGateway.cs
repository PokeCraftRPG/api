using PokeGame.Core.Identity.Models;
using Krakenar.Contracts.Passwords;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface IOneTimePasswordGateway
{
  Task<OneTimePassword> CreateMultiFactorAuthenticationAsync(User user, CancellationToken cancellationToken = default);
  Task<User> ValidateMultiFactorAuthenticationAsync(OneTimePasswordValidation validation, CancellationToken cancellationToken = default);
}
