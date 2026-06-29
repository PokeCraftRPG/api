using Krakenar.Contracts.Users;

namespace PokeGame.Core.Caching;

public interface ICacheService
{
  User? GetUser(Guid id);
  void RemoveUser(Guid id);
  void SetUser(User user);
}
