using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class SpeciesQuerier : ISpeciesQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<RegionalNumberEntity> _regionalNumbers;
  private readonly DbSet<SpeciesEntity> _species;

  public SpeciesQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _regionalNumbers = pokemon.RegionalNumbers;
    _species = pokemon.Species;
  }

  public async Task EnsureUnicityAsync(SpeciesAggregate species, CancellationToken cancellationToken)
  {
    Slug? key = null;
    Number? number = null;
    Dictionary<RegionId, Number> regionalNumbers = new(capacity: species.RegionalNumbers.Count);

    foreach (IEvent change in species.Changes)
    {
      if (change is SpeciesCreated created)
      {
        key = created.Key;
        number = created.Number;
      }
      else if (change is SpeciesKeyChanged changed)
      {
        key = changed.Key;
      }
      else if (change is SpeciesRegionalNumberChanged regionalNumber && regionalNumber.Number is not null)
      {
        regionalNumbers[regionalNumber.RegionId] = regionalNumber.Number;
      }
    }

    if (key is not null)
    {
      string? streamId = await _species.Where(x => x.World!.Id == species.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new PropertyConflictException<string>(species, new SpeciesId(streamId).EntityId, key.Value, nameof(SpeciesAggregate.Key));
      }
    }

    if (number is not null)
    {
      string? streamId = await _species.Where(x => x.World!.Id == species.WorldId.ToGuid() && x.Number == number.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new PropertyConflictException<int>(species, new SpeciesId(streamId).EntityId, number.Value, nameof(SpeciesAggregate.Number));
      }
    }

    foreach (KeyValuePair<RegionId, Number> regionalNumber in regionalNumbers)
    {
      string? streamId = await _regionalNumbers.Where(x => x.Region!.StreamId == regionalNumber.Key.Value && x.Number == regionalNumber.Value.Value)
        .Select(x => x.Species!.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != species.Id.Value)
      {
        throw new RegionalNumberConflictException(species, new SpeciesId(streamId), regionalNumber.Key, regionalNumber.Value, nameof(SpeciesAggregate.RegionalNumbers));
      }
    }
  }

  public async Task<SpeciesId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string? streamId = await _species.Where(x => x.World!.Id == _context.WorldUid && x.Key == Slug.Normalize(key))
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new SpeciesId(streamId);
  }

  public async Task<SpeciesModel> ReadAsync(SpeciesAggregate species, CancellationToken cancellationToken)
  {
    return await ReadAsync(species.Id, cancellationToken) ?? throw new InvalidOperationException($"The species entity '{species}' was not found.");
  }
  public async Task<SpeciesModel?> ReadAsync(SpeciesId id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }
  public async Task<SpeciesModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    SpeciesEntity? species = await _species.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.RegionalNumbers).ThenInclude(x => x.Region)
      .SingleOrDefaultAsync(cancellationToken);
    return species is null ? null : await MapAsync(species, cancellationToken);
  }

  private async Task<SpeciesModel> MapAsync(SpeciesEntity species, CancellationToken cancellationToken)
  {
    return (await MapAsync([species], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<SpeciesModel>> MapAsync(IEnumerable<SpeciesEntity> species, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = species.SelectMany(s => s.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return species.Select(mapper.ToSpecies).ToList().AsReadOnly();
  }
}
