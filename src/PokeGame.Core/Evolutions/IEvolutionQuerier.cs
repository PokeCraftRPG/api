using Krakenar.Contracts.Search;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Evolutions;

public interface IEvolutionQuerier
{
  Task EnsureDifferentSpeciesAsync(IEnumerable<Form> forms, CancellationToken cancellationToken = default);

  Task<EvolutionModel> ReadAsync(Evolution evolution, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(EvolutionId id, CancellationToken cancellationToken = default);
  Task<EvolutionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<SearchResults<EvolutionModel>> SearchAsync(SearchEvolutionsPayload payload, CancellationToken cancellationToken = default);
}
