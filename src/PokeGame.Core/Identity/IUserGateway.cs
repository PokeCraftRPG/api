using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface IUserGateway
{
  Task<User> CreateAsync(Email email, CancellationToken cancellationToken = default);
  Task<User?> FindAsync(Guid id, CancellationToken cancellationToken = default);
  Task<User?> FindAsync(string emailAddress, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
  Task<User> UpdateEmailAsync(User user, IEmail email, CancellationToken cancellationToken = default);
}
