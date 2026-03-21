namespace PokeGame.Core.Storages;

public interface IStorageService
{
  Task ExecuteWithQuotaAsync(IEntityProvider resource, Func<Task>? function = null, CancellationToken cancellationToken = default);
}

internal class StorageService : IStorageService
{
  public async Task ExecuteWithQuotaAsync(IEntityProvider resource, Func<Task>? function, CancellationToken cancellationToken)
  {
    // TODO(fpion): implement

    if (function is not null)
    {
      await function();
    }

    // TODO(fpion): implement
  }
}
