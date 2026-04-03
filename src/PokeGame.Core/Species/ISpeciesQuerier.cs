using Krakenar.Contracts.Search;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

public interface ISpeciesQuerier
{
  Task EnsureUnicityAsync(SpeciesAggregate species, CancellationToken cancellationToken = default);

  Task<SpeciesId?> FindIdAsync(string key, CancellationToken cancellationToken = default);

  Task<SpeciesModel> ReadAsync(SpeciesAggregate species, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(SpeciesId id, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken = default);
}
