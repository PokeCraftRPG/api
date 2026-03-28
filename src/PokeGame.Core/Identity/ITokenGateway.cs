using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface ITokenGateway
{
  Task<string> CreateEmailVerificationAsync(string emailAddress, CancellationToken cancellationToken = default);
  Task<string> CreateEmailVerificationAsync(User user, CancellationToken cancellationToken = default);
  Task<string> CreateProfileCompletionAsync(User user, CancellationToken cancellationToken = default);
}
