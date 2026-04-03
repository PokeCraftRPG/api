using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon;

public interface IPokemonQuerier
{
  Task EnsureUnicityAsync(Specimen specimen, CancellationToken cancellationToken = default);

  Task<PokemonModel> ReadAsync(Specimen specimen, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReadAsync(SpecimenId id, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<PokemonModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
