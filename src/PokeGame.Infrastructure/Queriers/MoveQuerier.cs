using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.Data;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Moves.Models;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class MoveQuerier : IMoveQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<MoveEntity> _moves;
  private readonly ISqlHelper _sql;

  public MoveQuerier(IActorService actors, IContext context, PokemonContext pokemon, ISqlHelper sql)
  {
    _actors = actors;
    _context = context;
    _moves = pokemon.Moves;
    _sql = sql;
  }

  public async Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in move.Changes)
    {
      if (change is MoveCreated created)
      {
        key = created.Key;
      }
      else if (change is MoveKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      string? streamId = await _moves.Where(x => x.World!.Id == move.WorldId.ToGuid() && x.Key == key.Value)
        .Select(x => x.StreamId)
        .SingleOrDefaultAsync(cancellationToken);
      if (streamId is not null && streamId != move.Id.Value)
      {
        throw new PropertyConflictException<string>(move, new MoveId(streamId).EntityId, key.Value, nameof(Move.Key));
      }
    }
  }

  public async Task<MoveId?> FindIdAsync(string key, CancellationToken cancellationToken)
  {
    string normalized = Slug.Normalize(key);
    string? streamId = await _moves.Where(x => x.World!.Id == _context.WorldUid && x.Key == normalized)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MoveId(streamId);
  }

  public async Task<IReadOnlyCollection<MoveKey>> ListKeysAsync(CancellationToken cancellationToken)
  {
    var keys = await _moves.Where(x => x.World!.Id == _context.WorldUid)
      .Select(x => new { x.StreamId, x.Id, x.Key })
      .ToArrayAsync(cancellationToken);
    return keys.Select(key => new MoveKey(new MoveId(key.StreamId), key.Id, key.Key)).ToList().AsReadOnly();
  }

  public async Task<MoveModel> ReadAsync(Move move, CancellationToken cancellationToken)
  {
    return await ReadAsync(move.Id, cancellationToken) ?? throw new InvalidOperationException($"The move entity '{move}' was not found.");
  }
  public async Task<MoveModel?> ReadAsync(MoveId id, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.StreamId == id.Value && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.Id == id && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.Key == Slug.Normalize(key) && x.World!.Id == _context.WorldUid)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }

  public async Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken)
  {
    IQueryBuilder builder = _sql.Query(PokemonDb.Moves.Table).SelectAll(PokemonDb.Moves.Table)
      .Join(PokemonDb.Worlds.WorldId, PokemonDb.Moves.WorldId)
      .ApplyWorldFilter(_context.WorldUid)
      .ApplyIdFilter(PokemonDb.Moves.Id, payload.Ids);
    _sql.ApplyTextSearch(builder, payload.Search, PokemonDb.Moves.Key, PokemonDb.Moves.Name);

    if (payload.Type.HasValue)
    {
      builder.Where(PokemonDb.Moves.Type, Operators.IsEqualTo(payload.Type.Value.ToString()));
    }
    if (payload.Category.HasValue)
    {
      builder.Where(PokemonDb.Moves.Category, Operators.IsEqualTo(payload.Category.Value.ToString()));
    }

    IQueryable<MoveEntity> query = _moves.FromQuery(builder).AsNoTracking();

    long total = await query.LongCountAsync(cancellationToken);

    IOrderedQueryable<MoveEntity>? ordered = null;
    foreach (MoveSortOption sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case MoveSort.Accuracy:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Accuracy) : query.OrderBy(x => x.Accuracy))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Accuracy) : ordered.ThenBy(x => x.Accuracy));
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
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Name) : ordered.ThenBy(x => x.Name));
          break;
        case MoveSort.Power:
          ordered = (ordered is null)
            ? (sort.IsDescending ? query.OrderByDescending(x => x.Power) : query.OrderBy(x => x.Power))
            : (sort.IsDescending ? ordered.ThenByDescending(x => x.Power) : ordered.ThenBy(x => x.Power));
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

    MoveEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<MoveModel> moves = await MapAsync(entities, cancellationToken);

    return new SearchResults<MoveModel>(moves, total);
  }

  private async Task<MoveModel> MapAsync(MoveEntity move, CancellationToken cancellationToken)
  {
    return (await MapAsync([move], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MoveModel>> MapAsync(IEnumerable<MoveEntity> moves, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = moves.SelectMany(move => move.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return moves.Select(mapper.ToMove).ToList().AsReadOnly();
  }
}
