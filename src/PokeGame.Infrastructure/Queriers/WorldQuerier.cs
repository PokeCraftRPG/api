using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class WorldQuerier : IWorldQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<WorldEntity> _worlds;

  public WorldQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _worlds = pokemon.Worlds;
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken)
  {
    return await _worlds.CountAsync(x => x.OwnerId == _context.UserId.Value, cancellationToken);
  }

  public async Task<WorldId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _worlds
      .Where(x => x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new WorldId(streamId);
  }

  public async Task<WorldDto> ReadAsync(World world, CancellationToken cancellationToken)
  {
    return await ReadAsync(world.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The world entity 'StreamId={world.Id}' was not found.");
  }
  public async Task<WorldDto?> ReadAsync(WorldId id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Id == id && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }
  public async Task<WorldDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    WorldEntity? world = await _worlds.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.OwnerId == _context.UserId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return world is null ? null : await MapAsync(world, cancellationToken);
  }

  public async Task<SearchResults<WorldDto>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<WorldEntity> query = _worlds.AsNoTracking()
      .Where(x => x.OwnerId == _context.UserId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => world
        => EF.Functions.ILike(world.Key, pattern, @"\")
        || EF.Functions.ILike(world.Name!, pattern, @"\")
        || EF.Functions.ILike(world.Summary!, pattern, @"\"));

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<WorldDto>(total);
    }

    IOrderedQueryable<WorldEntity>? ordered = null;
    foreach (SortOption<WorldSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case WorldSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case WorldSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case WorldSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case WorldSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.WorldId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    WorldEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<WorldDto> worlds = await MapAsync(entities, cancellationToken);

    return new SearchResults<WorldDto>(worlds, total);
  }

  private async Task<WorldDto> MapAsync(WorldEntity world, CancellationToken cancellationToken)
  {
    return (await MapAsync([world], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<WorldDto>> MapAsync(IEnumerable<WorldEntity> worlds, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = worlds.SelectMany(world => world.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return worlds.Select(mapper.ToWorld).ToList().AsReadOnly();
  }
}
