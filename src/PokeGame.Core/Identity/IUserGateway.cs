using Krakenar.Contracts.Users;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

public interface IUserGateway
{
  Task<User> AuthenticateAsync(string uniqueName, string password, CancellationToken cancellationToken = default);
  Task<User> AuthenticateAsync(User user, string password, CancellationToken cancellationToken = default);
  Task<User> CompleteProfileAsync(Guid id, CompleteProfilePayload profile, PhonePayload? phone = null, CancellationToken cancellationToken = default);
  Task<User> CreateAsync(Email email, CancellationToken cancellationToken = default);
  Task<User?> FindAsync(Guid id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<User>> FindAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
  Task<User?> FindAsync(string uniqueName, CancellationToken cancellationToken = default);
  Task<User> SignOutAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<User> UpdateEmailAsync(User user, Email email, CancellationToken cancellationToken = default);
  Task<User> UpdateProfileAsync(Guid id, UpdateProfilePayload profile, CancellationToken cancellationToken = default);
}
