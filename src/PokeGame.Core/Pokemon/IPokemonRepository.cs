namespace PokeGame.Core.Pokemon;

public interface IPokemonRepository
{
  Task<Specimen?> LoadAsync(PokemonId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Specimen>> LoadAsync(IEnumerable<PokemonId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Specimen specimen, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Specimen> specimens, CancellationToken cancellationToken = default);
}
