namespace PokeGame.Core.Species;

public interface ISpeciesRepository
{
  Task<SpeciesAggregate?> LoadAsync(SpeciesId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<SpeciesAggregate>> LoadAsync(IEnumerable<SpeciesId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(SpeciesAggregate species, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<SpeciesAggregate> species, CancellationToken cancellationToken = default);
}
