using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon.Queries;

internal record ReadPokemonQuery(Guid? Id, string? Key) : IQuery<PokemonModel?>;

internal class ReadPokemonQueryHandler : IQueryHandler<ReadPokemonQuery, PokemonModel?>
{
  private readonly IPokemonQuerier _pokemonQuerier;

  public ReadPokemonQueryHandler(IPokemonQuerier pokemonQuerier)
  {
    _pokemonQuerier = pokemonQuerier;
  }

  public async Task<PokemonModel?> HandleAsync(ReadPokemonQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, PokemonModel> pokemons = new(capacity: 2);

    if (query.Id.HasValue)
    {
      PokemonModel? pokemon = await _pokemonQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (pokemon is not null)
      {
        pokemons[pokemon.Id] = pokemon;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      PokemonModel? pokemon = await _pokemonQuerier.ReadAsync(query.Key, cancellationToken);
      if (pokemon is not null)
      {
        pokemons[pokemon.Id] = pokemon;
      }
    }

    if (pokemons.Count > 1)
    {
      throw TooManyResultsException<PokemonModel>.ExpectedSingle(pokemons.Count);
    }

    return pokemons.Values.SingleOrDefault();
  }
}
