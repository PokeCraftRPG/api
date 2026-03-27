using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface IUserGateway
{
  Task<User?> FindAsync(string emailAddress, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
