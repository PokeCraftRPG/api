using Logitar.EventSourcing;
using PokeGame.Core.Evolutions;

namespace PokeGame.Infrastructure.Repositories;

internal class EvolutionRepository : Repository, IEvolutionRepository
{
  public EvolutionRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Evolution?> LoadAsync(EvolutionId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Evolution>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Evolution>> LoadAsync(IEnumerable<EvolutionId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Evolution>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Evolution evolution, CancellationToken cancellationToken)
  {
    await base.SaveAsync(evolution, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Evolution> evolutions, CancellationToken cancellationToken)
  {
    await base.SaveAsync(evolutions, cancellationToken);
  }
}
