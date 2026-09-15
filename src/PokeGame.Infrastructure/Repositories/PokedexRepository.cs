using Logitar.EventSourcing;
using PokeGame.Core.Pokedexes;

namespace PokeGame.Infrastructure.Repositories;

internal class PokedexRepository : Repository, IPokedexRepository
{
  public PokedexRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<Pokedex?> LoadAsync(PokedexId id, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Pokedex>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<Pokedex>> LoadAsync(IEnumerable<PokedexId> ids, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<Pokedex>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(Pokedex pokedex, CancellationToken cancellationToken)
  {
    await base.SaveAsync(pokedex, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<Pokedex> pokedexes, CancellationToken cancellationToken)
  {
    await base.SaveAsync(pokedexes, cancellationToken);
  }
}
