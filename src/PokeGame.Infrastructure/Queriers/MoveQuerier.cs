using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class MoveQuerier : IMoveQuerier
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly DbSet<MoveEntity> _moves;
  private readonly ISqlHelper _sqlHelper;

  public MoveQuerier(IActorService actorService, IContext context, PokemonContext pokemon, ISqlHelper sqlHelper)
  {
    _actorService = actorService;
    _context = context;
    _moves = pokemon.Moves;
    _sqlHelper = sqlHelper;
  }

  public async Task<MoveModel> FindAsync(Move move, CancellationToken cancellationToken)
  {
    return await FindAsync(move.Id, cancellationToken);
  }
  public async Task<MoveModel> FindAsync(MoveId id, CancellationToken cancellationToken)
  {
    MoveEntity move = await _moves.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException($"The move entity 'StreamId={id}' was not found.");
    return await MapAsync(move, cancellationToken);
  }

  public async Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.EntityId == id && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.StreamId == _context.WorldId.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }

  public async Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Moves.Table).SelectAll(Db.Moves.Table)
      .ApplyWorldFilter(Db.Moves.WorldId, _context.WorldId)
      .ApplyIdFilter(Db.Moves.EntityId, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Moves.Key, Db.Moves.Name);

    IQueryable<MoveEntity> query = _moves.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<MoveEntity>? ordered = null;
    foreach (MoveSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case MoveSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case MoveSort.Key:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case MoveSort.Name:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case MoveSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered ?? query;

    query = query.ApplyPaging(payload);

    MoveEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<MoveModel> moves = await MapAsync(entities, cancellationToken);

    return new SearchResults<MoveModel>(moves, total);
  }

  public async Task<MoveId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken)
  {
    string? streamId = await _moves
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MoveId(streamId);
  }

  private async Task<MoveModel> MapAsync(MoveEntity move, CancellationToken cancellationToken)
  {
    return (await MapAsync([move], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MoveModel>> MapAsync(IEnumerable<MoveEntity> moves, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = moves.SelectMany(move => move.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actorService.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return moves.Select(mapper.ToMove).ToList().AsReadOnly();
  }
}
