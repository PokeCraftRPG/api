using Krakenar.Contracts.Sessions;
using Krakenar.Contracts.Tokens;
using Krakenar.Contracts.Users;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public interface ITokenGateway
{
  Task<string> CreateEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken = default);
  Task<string> CreateEmailVerificationAsync(User user, CancellationToken cancellationToken = default);
  Task<string> CreateProfileCompletionAsync(User user, CancellationToken cancellationToken = default);
  Task<TokenResponse> GetResponseAsync(Session session, CancellationToken cancellationToken = default);
  Task<User> ValidateAccessAsync(string token, CancellationToken cancellationToken = default);
  Task<ValidatedToken> ValidateEmailVerificationAsync(string token, CancellationToken cancellationToken = default);
  Task<ValidatedToken> ValidateProfileCompletionAsync(string token, CancellationToken cancellationToken = default);
}
