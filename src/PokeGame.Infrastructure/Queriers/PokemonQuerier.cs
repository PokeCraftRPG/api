using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Infrastructure.Queriers;

internal class PokemonQuerier : IPokemonQuerier
{
  public Task EnsureUnicityAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<PokemonModel> ReadAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
  public Task<PokemonModel?> ReadAsync(SpecimenId id, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
  public Task<PokemonModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
  public Task<PokemonModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
