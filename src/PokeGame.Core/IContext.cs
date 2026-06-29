namespace PokeGame.Core;

public interface IContext
{
  Guid UserId { get; }
  Guid WorldId { get; }

  bool IsWorldOwner();

  Guid? TryGetUserId();
  Guid? TryGetWorldId();

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
