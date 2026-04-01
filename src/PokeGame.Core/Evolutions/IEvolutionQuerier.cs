using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions;

public interface IEvolutionQuerier
{
  Task<EvolutionModel> ReadAsync(Evolution evolution, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(EvolutionId id, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
