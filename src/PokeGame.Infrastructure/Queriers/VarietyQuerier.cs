using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Events;
using PokeGame.Core.Varieties.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class VarietyQuerier : IVarietyQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<VarietyEntity> _varieties;

  public VarietyQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _varieties = pokemon.Varieties;
  }

  public async Task EnsureUnicityAsync(Variety variety, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in variety.Changes)
    {
      if (change is VarietyCreated created)
      {
        key = created.Key;
      }
      else if (change is VarietyKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _varieties.Where(x => x.World!.Id == variety.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != variety.Id.Value)
      {
        throw new PropertyConflictException<string>(variety, new VarietyId(streamId).EntityId, key.Value, nameof(Variety.Key));
      }
    }
  }

  public async Task<VarietyId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string normalized = Slug.Normalize(key);
    string? streamId = await _varieties.Where(x => x.World!.Id == _context.WorldUid && x.Key == normalized)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new VarietyId(streamId);
  }

  public async Task<VarietyModel> ReadAsync(Variety variety, CancellationToken cancellationToken)
  {
    return await ReadAsync(variety.Id, cancellationToken) ?? throw new InvalidOperationException($"The variety entity '{variety}' was not found.");
  }
  public async Task<VarietyModel?> ReadAsync(VarietyId id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }
  public async Task<VarietyModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    VarietyEntity? variety = await _varieties.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .Include(x => x.Species!).ThenInclude(x => x!.RegionalNumbers).ThenInclude(x => x.Region)
      .Include(x => x.Moves).ThenInclude(x => x.Move)
      .SingleOrDefaultAsync(cancellationToken);
    return variety is null ? null : await MapAsync(variety, cancellationToken);
  }

  private async Task<VarietyModel> MapAsync(VarietyEntity variety, CancellationToken cancellationToken)
  {
    return (await MapAsync([variety], cancellationToken)).Single();
  }

  private async Task<IReadOnlyCollection<VarietyModel>> MapAsync(IEnumerable<VarietyEntity> varieties, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = varieties.SelectMany(variety => variety.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return varieties.Select(mapper.ToVariety).ToList().AsReadOnly();
  }
}
