using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Seo;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class PokemonQuerier : IPokemonQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<PokemonEntity> _pokemon;

  public PokemonQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _pokemon = pokemon.Specimens;
  }

  public async Task<PokemonId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _pokemon
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new PokemonId(streamId);
  }

  public async Task<PokemonDto> ReadAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    return await ReadAsync(specimen.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The Pokémon entity 'StreamId={specimen.Id}' was not found.");
  }
  public async Task<PokemonDto?> ReadAsync(PokemonId id, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }
  public async Task<PokemonDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }
  public async Task<PokemonDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .IncludeRelated()
      .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }

  private async Task<PokemonDto> MapAsync(PokemonEntity pokemon, CancellationToken cancellationToken)
  {
    return (await MapAsync([pokemon], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<PokemonDto>> MapAsync(IEnumerable<PokemonEntity> pokemon, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = pokemon.SelectMany(entity => entity.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return pokemon.Select(mapper.ToPokemon).ToList().AsReadOnly();
  }
}
