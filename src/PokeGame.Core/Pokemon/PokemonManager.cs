using Logitar.EventSourcing;
using PokeGame.Core.Pokemon.Events;

namespace PokeGame.Core.Pokemon;

public interface IPokemonManager
{
  Task EnsureUnicityAsync(Specimen specimen, CancellationToken cancellationToken = default);
}

internal class PokemonManager : IPokemonManager
{
  private readonly IPokemonQuerier _pokemonQuerier;

  public PokemonManager(IPokemonQuerier pokemonQuerier)
  {
    _pokemonQuerier = pokemonQuerier;
  }

  public async Task EnsureUnicityAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in specimen.Changes)
    {
      if (change is PokemonCreated created)
      {
        key = created.Key;
      }
      else if (change is PokemonKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      PokemonId? pokemonId = await _pokemonQuerier.GetIdAsync(key, cancellationToken);
      if (pokemonId.HasValue && !pokemonId.Value.Equals(specimen.Id))
      {
        throw new KeyAlreadyUsedException(specimen, pokemonId.Value.EntityId, specimen.Key, nameof(specimen.Key));
      }
    }
  }
}
