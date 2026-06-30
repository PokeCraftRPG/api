using Krakenar.Contracts.Search;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

public interface ISpeciesRepository
{
  void Add(params PokemonSpecies[] species);
  void Remove(PokemonSpecies species);
  void Update(PokemonSpecies species, PokemonSpeciesUpdated record);

  Task EnsureUnicityAsync(PokemonSpecies species, CancellationToken cancellationToken = default);

  Task<PokemonSpecies?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<SpeciesModel> ReadAsync(PokemonSpecies species, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(int number, CancellationToken cancellationToken = default);
  Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<SpeciesModel>> SearchAsync(SearchSpeciesPayload payload, CancellationToken cancellationToken = default);
}
