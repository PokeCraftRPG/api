using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class RegionQuerier : IRegionQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<RegionEntity> _regions;

  public RegionQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _regions = pokemon.Regions;
  }

  public async Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in region.Changes)
    {
      if (change is RegionCreated created)
      {
        key = created.Key;
      }
      else if (change is RegionKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _regions.Where(x => x.World!.Id == region.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != region.Id.Value)
      {
        throw new PropertyConflictException<string>(region, new RegionId(streamId).EntityId, key.Value, nameof(Region.Key));
      }
    }
  }

  public async Task<IReadOnlyCollection<RegionKey>> ListKeysAsync(CancellationToken cancellationToken)
  {
    var keys = await _regions.Where(x => x.World!.Id == _context.WorldUid)
      .Select(x => new { x.StreamId, x.Id, x.Key })
      .ToArrayAsync(cancellationToken);
    return keys.Select(key => new RegionKey(new RegionId(key.StreamId), key.Id, key.Key)).ToList().AsReadOnly();
  }

  public async Task<RegionModel> ReadAsync(Region region, CancellationToken cancellationToken)
  {
    return await ReadAsync(region.Id, cancellationToken) ?? throw new InvalidOperationException($"The region entity '{region}' was not found.");
  }
  public async Task<RegionModel?> ReadAsync(RegionId id, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _regions.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return region is null ? null : await MapAsync(region, cancellationToken);
  }
  public async Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _regions.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return region is null ? null : await MapAsync(region, cancellationToken);
  }
  public async Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    RegionEntity? region = await _regions.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return region is null ? null : await MapAsync(region, cancellationToken);
  }

  private async Task<RegionModel> MapAsync(RegionEntity region, CancellationToken cancellationToken)
  {
    return (await MapAsync([region], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<RegionModel>> MapAsync(IEnumerable<RegionEntity> regions, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = regions.SelectMany(region => region.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return regions.Select(mapper.ToRegion).ToList().AsReadOnly();
  }
}
