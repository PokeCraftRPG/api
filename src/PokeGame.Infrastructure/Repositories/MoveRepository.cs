using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Moves.Models;
using PokeGame.Infrastructure.Actors;

namespace PokeGame.Infrastructure.Repositories;

internal class MoveRepository : Repository, IMoveRepository
{
  private readonly IActorService _actorService;
  private readonly IContext _context;
  private readonly ISqlHelper _sqlHelper;

  public MoveRepository(IActorService actorService, IContext context, PokemonContext database, ISqlHelper sqlHelper) : base(database)
  {
    _actorService = actorService;
    _context = context;
    _sqlHelper = sqlHelper;
  }

  public void Add(params Move[] moves)
  {
    foreach (Move move in moves)
    {
      Database.Moves.Add(move);
      base.RecordChange(new MoveCreated(move));
    }
  }
  public void Remove(Move move)
  {
    Database.Moves.Remove(move);
    base.RecordChange(new MoveDeleted(move, _context.UserId));
  }
  public void Update(Move move, MoveUpdated record)
  {
    Database.Moves.Update(move);
    base.RecordChange(record);
  }

  public async Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken)
  {
    Guid? moveId = await Database.Moves.Where(x => x.Key == move.Key && x.WorldId == _context.WorldId)
      .Select(x => (Guid?)x.Id)
      .SingleOrDefaultAsync(cancellationToken);
    if (moveId.HasValue && !moveId.Value.Equals(move.Id))
    {
      throw new KeyAlreadyUsedException(move, moveId.Value, move.Key, nameof(Move.Key));
    }
  }

  public async Task<Move?> LoadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await Database.Moves.SingleOrDefaultAsync(x => x.Id == id && x.WorldId == _context.WorldId, cancellationToken);
  }

  public async Task<MoveModel> ReadAsync(Move move, CancellationToken cancellationToken)
  {
    return await ReadAsync(move.Id, cancellationToken) ?? throw new InvalidOperationException($"The move 'Id={move.Id}' was not found.");
  }
  public async Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    Move? move = await Database.Moves.AsNoTracking()
      .Where(x => x.Id == id && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    Move? move = await Database.Moves.AsNoTracking()
      .Where(x => x.Key == SlugHelper.Format(key) && x.WorldId == _context.WorldId)
      .SingleOrDefaultAsync(cancellationToken);

    return move is null ? null : await MapAsync(move, cancellationToken);
  }

  public async Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sqlHelper.Query(Db.Moves.Table).SelectAll(Db.Moves.Table)
      .Where(Db.Moves.WorldId, Operators.IsEqualTo(_context.WorldId))
      .ApplyIdFilter(Db.Moves.Id, payload.Ids);
    _sqlHelper.ApplyTextSearch(builder, payload.Search, Db.Moves.Name);

    if (payload.Type.HasValue)
    {
      builder.Where(Db.Moves.Type, Operators.IsEqualTo(payload.Type.Value.ToString()));
    }
    if (payload.Category.HasValue)
    {
      builder.Where(Db.Moves.Category, Operators.IsEqualTo(payload.Category.Value.ToString()));
    }

    IQueryable<Move> query = Database.Moves.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<Move>? ordered = null;
    foreach (MoveSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case MoveSort.Accuracy:
          ordered = (ordered is null)
            ? (sort.IsDescending
              ? query.OrderByDescending(x => x.Accuracy.HasValue).ThenByDescending(x => x.Accuracy)
              : query.OrderByDescending(x => x.Accuracy.HasValue).ThenBy(x => x.Accuracy))
            : (sort.IsDescending
              ? ordered.ThenByDescending(x => x.Accuracy.HasValue).ThenByDescending(x => x.Accuracy)
              : ordered.ThenByDescending(x => x.Accuracy.HasValue).ThenBy(x => x.Accuracy));
          break;
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
        case MoveSort.Power:
          ordered = (ordered is null)
            ? (sort.IsDescending
              ? query.OrderByDescending(x => x.Power.HasValue).ThenByDescending(x => x.Power)
              : query.OrderByDescending(x => x.Power.HasValue).ThenBy(x => x.Power))
            : (sort.IsDescending
              ? ordered.ThenByDescending(x => x.Power.HasValue).ThenByDescending(x => x.Power)
              : ordered.ThenByDescending(x => x.Power.HasValue).ThenBy(x => x.Power));
          break;
        case MoveSort.PowerPoints:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.PowerPoints) : query.OrderBy(x => x.PowerPoints))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.PowerPoints) : ordered.ThenBy(x => x.PowerPoints));
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

    Move[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<MoveModel> moves = await MapAsync(entities, cancellationToken);

    return new SearchResults<MoveModel>(moves, total);
  }

  private async Task<MoveModel> MapAsync(Move move, CancellationToken cancellationToken)
  {
    return (await MapAsync([move], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MoveModel>> MapAsync(IEnumerable<Move> moves, CancellationToken cancellationToken)
  {
    IEnumerable<Guid> userIds = moves.SelectMany(move => move.GetUserIds());
    IReadOnlyDictionary<Guid, Actor> actors = await _actorService.FindAsync(userIds, cancellationToken);
    Mapper mapper = new(actors);

    return moves.Select(mapper.ToMove).ToList().AsReadOnly();
  }
}
