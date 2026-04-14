namespace PokeGame.Core.Species;

public interface ISpeciesRepository
{
  Task<PokemonSpecies?> LoadAsync(SpeciesId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<PokemonSpecies>> LoadAsync(IEnumerable<SpeciesId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(PokemonSpecies species, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<PokemonSpecies> species, CancellationToken cancellationToken = default);
}
