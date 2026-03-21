namespace PokeGame.Core.Permissions;

public interface IPermissionService
{
  Task CheckAsync(string action, CancellationToken cancellationToken = default);
  Task CheckAsync(string action, IEntityProvider? resource, CancellationToken cancellationToken = default);
}

internal class PermissionService : IPermissionService
{
  public async Task CheckAsync(string action, CancellationToken cancellationToken)
  {
    await CheckAsync(action, resource: null, cancellationToken);
  }
  public async Task CheckAsync(string action, IEntityProvider? resource, CancellationToken cancellationToken)
  {
    // TODO(fpion): implement
  }
}
