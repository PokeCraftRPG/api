using Logitar.EventSourcing;
using PokeGame.Core.Species;

namespace PokeGame.Infrastructure.Repositories;

internal class SpeciesRepository : Repository, ISpeciesRepository
{
  public SpeciesRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<SpeciesAggregate?> LoadAsync(SpeciesId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<SpeciesAggregate>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<SpeciesAggregate>> LoadAsync(IEnumerable<SpeciesId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<SpeciesAggregate>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(SpeciesAggregate species, CancellationToken cancellationToken)
  {
    await base.SaveAsync(species, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<SpeciesAggregate> species, CancellationToken cancellationToken)
  {
    await base.SaveAsync(species, cancellationToken);
  }
}
