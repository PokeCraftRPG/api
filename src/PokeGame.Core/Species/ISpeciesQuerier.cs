using Krakenar.Contracts.Search;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

public interface ISpeciesQuerier
{
  Task<SpeciesId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);
  Task<SpeciesId?> GetIdAsync(Number number, CancellationToken cancellationToken = default);

  Task<SpeciesDto> ReadAsync(PokemonSpecies species, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(SpeciesId id, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(int number, CancellationToken cancellationToken = default);
  Task<SpeciesDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<SpeciesDto>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken = default);
}
