using Krakenar.Contracts.Search;
using PokeGame.Core.Evolutions.Models;

namespace PokeGame.Core.Evolutions;

public interface IEvolutionQuerier
{
  Task<EvolutionDto> ReadAsync(Evolution evolution, CancellationToken cancellationToken = default);
  Task<EvolutionDto?> ReadAsync(EvolutionId id, CancellationToken cancellationToken = default);
  Task<EvolutionDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<SearchResults<EvolutionDto>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken = default);
}
