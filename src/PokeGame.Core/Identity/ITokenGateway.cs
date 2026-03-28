using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface ITokenGateway
{
  Task<string> CreateEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken = default);
  Task<string> CreateEmailVerificationAsync(User user, CancellationToken cancellationToken = default);
  Task<ValidatedToken> ValidateEmailVerificationAsync(string token, CancellationToken cancellationToken = default);

  Task<string> CreateProfileCompletionAsync(User user, CancellationToken cancellationToken = default);
}
