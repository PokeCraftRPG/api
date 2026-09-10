using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon;

public interface IPokemonQuerier
{
  Task<PokemonId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<PokemonDto> ReadAsync(Specimen pokemon, CancellationToken cancellationToken = default);
  Task<PokemonDto?> ReadAsync(PokemonId id, CancellationToken cancellationToken = default);
  Task<PokemonDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<PokemonDto?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
