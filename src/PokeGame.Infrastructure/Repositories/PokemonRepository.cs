using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Infrastructure.Repositories;

internal class PokemonRepository : Repository, IPokemonRepository
{
  public PokemonRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Specimen?> LoadAsync(SpecimenId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<Specimen>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Specimen>> LoadAsync(IEnumerable<SpecimenId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<Specimen>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    await base.SaveAsync(specimen, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Specimen> specimens, CancellationToken cancellationToken)
  {
    await base.SaveAsync(specimens, cancellationToken);
  }
}
