namespace PokeGame.Core.Pokedexes;

public interface IPokedexRepository
{
  Task<Pokedex?> LoadAsync(PokedexId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Pokedex>> LoadAsync(IEnumerable<PokedexId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Pokedex pokedex, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Pokedex> pokedexes, CancellationToken cancellationToken = default);
}
