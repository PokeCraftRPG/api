namespace PokeGame.Core.Evolutions;

public interface IEvolutionRepository
{
  Task<Evolution?> LoadAsync(EvolutionId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Evolution>> LoadAsync(IEnumerable<EvolutionId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Evolution evolution, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Evolution> evolutions, CancellationToken cancellationToken = default);
}

