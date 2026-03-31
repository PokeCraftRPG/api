using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core.Storages;
using PokeGame.Core.Storages.Events;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Handlers;

internal class StorageEvents : IEventHandler<EntityStored>, IEventHandler<StorageDeleted>, IEventHandler<StorageInitialized>
{
  public static void Register(IServiceCollection services)
  {
    services.AddTransient<IEventHandler<EntityStored>, StorageEvents>();
    services.AddTransient<IEventHandler<StorageDeleted>, StorageEvents>();
    services.AddTransient<IEventHandler<StorageInitialized>, StorageEvents>();
  }

  private readonly PokemonContext _pokemon;

  public StorageEvents(PokemonContext pokemon)
  {
    _pokemon = pokemon;
  }

  public async Task HandleAsync(EntityStored @event, CancellationToken cancellationToken)
  {
    StorageSummaryEntity? summary = await _pokemon.StorageSummary
      .Include(x => x.Detail)
      .SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (summary is not null && summary.Version == (@event.Version - 1))
    {
      summary.Store(@event);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(StorageDeleted @event, CancellationToken cancellationToken)
  {
    StorageSummaryEntity? summary = await _pokemon.StorageSummary.SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (summary is not null)
    {
      _pokemon.StorageSummary.Remove(summary);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task HandleAsync(StorageInitialized @event, CancellationToken cancellationToken)
  {
    StorageSummaryEntity? summary = await _pokemon.StorageSummary.AsNoTracking().SingleOrDefaultAsync(x => x.StreamId == @event.StreamId.Value, cancellationToken);
    if (summary is null)
    {
      StorageId storageId = new(@event.StreamId);
      WorldEntity world = await _pokemon.Worlds.SingleOrDefaultAsync(x => x.StreamId == storageId.WorldId.Value, cancellationToken)
        ?? throw new InvalidOperationException($"The world entity 'StreamId={storageId.WorldId}' was not found.");

      summary = new StorageSummaryEntity(world, @event);

      _pokemon.StorageSummary.Add(summary);

      await _pokemon.SaveChangesAsync(cancellationToken);
    }
  }
}
