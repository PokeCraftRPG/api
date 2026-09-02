using Krakenar.Contracts.Users;

namespace PokeGame.Core.Identity;

public interface IUserGateway
{
  Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
