using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon.Queries;

internal record ReadPokemonQuery(Guid? Id, string? Key) : IQuery<PokemonDto?>;

internal class ReadPokemonQueryHandler : IQueryHandler<ReadPokemonQuery, PokemonDto?>
{
  private readonly IPokemonQuerier _pokemonQuerier;

  public ReadPokemonQueryHandler(IPokemonQuerier pokemonQuerier)
  {
    _pokemonQuerier = pokemonQuerier;
  }

  public async Task<PokemonDto?> HandleAsync(ReadPokemonQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, PokemonDto> pokemon = new(capacity: 2);

    if (query.Id.HasValue)
    {
      PokemonDto? result = await _pokemonQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (result is not null)
      {
        pokemon[result.Id] = result;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      PokemonDto? result = await _pokemonQuerier.ReadAsync(query.Key, cancellationToken);
      if (result is not null)
      {
        pokemon[result.Id] = result;
      }
    }

    if (pokemon.Count > 1)
    {
      throw TooManyResultsException<PokemonDto>.ExpectedSingle(pokemon.Count);
    }

    return pokemon.Values.SingleOrDefault();
  }
}
