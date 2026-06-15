using Krakenar.Contracts.Search;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

public interface ISpeciesQuerier
{
  Task<SpeciesModel> FindAsync(PokemonSpecies species, CancellationToken cancellationToken = default);
  Task<SpeciesModel> FindAsync(SpeciesId id, CancellationToken cancellationToken = default);

  Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken = default);

  Task<SpeciesId?> TryGetIdAsync(Number number, RegionId? regionId = null, CancellationToken cancellationToken = default);
  Task<SpeciesId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken = default);
}
