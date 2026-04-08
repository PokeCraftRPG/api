using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class WorldQuerier : IWorldQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<WorldEntity> _worlds;
  private readonly ISqlHelper _sql;

  public WorldQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _worlds = pokemon.Worlds;
    _sql = sql;
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken)
  {
    return await _worlds.CountAsync(x => x.OwnerId == _context.UserId.Value, cancellationToken);
  }

  public async Task EnsureUnicityAsync(World world, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in world.Changes)
    {
      if (change is WorldCreated created)
      {
        key = created.Key;
      }
      else if (change is WorldKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _worlds.Where(x => x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != world.Id.Value)
      {
        throw new PropertyConflictException<string>(world, new WorldId(streamId).ToGuid(), key.Value, nameof(World.Key));
      }
    }
  }

  public async Task<WorldModel> ReadAsync(World world, CancellationToken cancellationToken)
  {
    return await ReadAsync(world.Id, cancellationToken) ?? throw new InvalidOperationException($"The world entity '{world}' was not found.");
  }
  public async Task<WorldModel?> ReadAsync(WorldId id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.StreamId == id.Value /*&& x.OwnerId == _context.UserId.Value*/)
      .Include(x => x.Members)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Id == id /*&& x.OwnerId == _context.UserId.Value*/)
      .Include(x => x.Members)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) /*&& x.OwnerId == _context.UserId.Value*/)
      .Include(x => x.Members)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }

  public async Task<SearchResults<WorldModel>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Worlds.Table).SelectAll(PokemonDb.Worlds.Table)
      .ApplyOwnerFilter(_context.UserId)
      .ApplyIdFilter(PokemonDb.Worlds.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Worlds.Key, PokemonDb.Worlds.Name);

    IQueryable<WorldEntity> query = _worlds.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<WorldEntity>? ordered = null;
    foreach (WorldSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case WorldSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case WorldSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case WorldSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case WorldSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    WorldEntity[] entities = await query.Include(x => x.Members).ToArrayAsync(cancellationToken);
    IReadOnlyCollection<WorldModel> worlds = await MapAsync(entities, cancellationToken);

    return new SearchResults<WorldModel>(worlds, total);
  }

  private async Task<WorldModel> MapAsync(WorldEntity world, CancellationToken cancellationToken)
  {
    return (await MapAsync([world], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<WorldModel>> MapAsync(IEnumerable<WorldEntity> worlds, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = worlds.SelectMany(world => world.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return worlds.Select(mapper.ToWorld).ToList().AsReadOnly();
  }
}
