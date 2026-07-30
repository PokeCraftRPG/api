using Krakenar.Contracts;

namespace PokeGame.Core;

public interface IContext
{
  Guid UserId { get; }
  Guid WorldId { get; }

  IReadOnlyCollection<CustomAttribute> GetSessionCustomAttributes();

  bool IsWorldOwner();

  Guid? TryGetSessionId();
  Guid? TryGetUserId();
  Guid? TryGetWorldId();

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
