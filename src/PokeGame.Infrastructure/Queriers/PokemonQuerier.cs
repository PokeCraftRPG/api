using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
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
    _pokemon = pokemon.Pokemon;
  }

  public async Task EnsureUnicityAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    Slug? key = null;

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
      string? streamId = await _pokemon.Where(x => x.World!.Id == specimen.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != specimen.Id.Value)
      {
        throw new PropertyConflictException<string>(specimen, new PokemonId(streamId).EntityId, key.Value, nameof(Specimen.Key));
      }
    }
  }

  public async Task<PokemonModel> ReadAsync(Specimen specimen, CancellationToken cancellationToken)
  {
    return await ReadAsync(specimen.Id, cancellationToken) ?? throw new InvalidOperationException($"The Pokémon entity '{specimen}' was not found.");
  }
  public async Task<PokemonModel?> ReadAsync(PokemonId id, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking().AsSplitQuery()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.CurrentTrainer)
      .Include(x => x.Form).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.OriginalTrainer)
      .Include(x => x.PokeBall)
      .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }
  public async Task<PokemonModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking().AsSplitQuery()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.CurrentTrainer)
      .Include(x => x.Form).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.OriginalTrainer)
      .Include(x => x.PokeBall)
      .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }
  public async Task<PokemonModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    PokemonEntity? pokemon = await _pokemon.AsNoTracking().AsSplitQuery()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.CurrentTrainer)
      .Include(x => x.Form).ThenInclude(x => x!.Abilities).ThenInclude(x => x.Ability)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Moves).ThenInclude(x => x.Move)
      .Include(x => x.Form).ThenInclude(x => x!.Variety).ThenInclude(x => x!.Species).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.HeldItem).ThenInclude(x => x!.Move)
      .Include(x => x.OriginalTrainer)
      .Include(x => x.PokeBall)
    .SingleOrDefaultAsync(cancellationToken);
    return pokemon is null ? null : await MapAsync(pokemon, cancellationToken);
  }

  private async Task<PokemonModel> MapAsync(PokemonEntity pokemon, CancellationToken cancellationToken)
  {
    return (await MapAsync([pokemon], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<PokemonModel>> MapAsync(IEnumerable<PokemonEntity> pokeomon, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = pokeomon.SelectMany(pokemon => pokemon.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return pokeomon.Select(mapper.ToPokemon).ToList().AsReadOnly();
  }
}
