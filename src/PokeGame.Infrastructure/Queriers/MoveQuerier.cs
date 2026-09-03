using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Logitar.EventSourcing;
using Microsoft.EntityFrameworkCore;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Infrastructure.Actors;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure.Queriers;

internal class MoveQuerier : IMoveQuerier
{
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<MoveEntity> _moves;

  public MoveQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _moves = pokemon.Moves;
  }

  public async Task<MoveId?> GetIdAsync(Key key, CancellationToken cancellationToken)
  {
    string? streamId = await _moves
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    return streamId is null ? null : new MoveId(streamId);
  }

  public async Task<MoveDto> ReadAsync(Move move, CancellationToken cancellationToken)
  {
    return await ReadAsync(move.Id, cancellationToken)
      ?? throw new InvalidOperationException($"The move entity 'StreamId={move.Id}' was not found.");
  }
  public async Task<MoveDto?> ReadAsync(MoveId id, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.StreamId == id.Value)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveDto?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Id == id)
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }
  public async Task<MoveDto?> ReadAsync(string key, CancellationToken cancellationToken)
  {
    MoveEntity? move = await _moves.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value && x.Key == SlugHelper.Format(key))
      .SingleOrDefaultAsync(cancellationToken);
    return move is null ? null : await MapAsync(move, cancellationToken);
  }

  public async Task<SearchResults<MoveDto>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken)
  {
    IQueryable<MoveEntity> query = _moves.AsNoTracking()
      .Where(x => x.World!.StreamId == _context.WorldId.Value)
      .ApplyIdFilter(payload.Ids, x => x.Id)
      .ApplyTextSearch(payload.Search, pattern => move
        => EF.Functions.ILike(move.Key, pattern, @"\")
        || EF.Functions.ILike(move.Name!, pattern, @"\")
        || EF.Functions.ILike(move.Summary!, pattern, @"\"));

    if (payload.Type.HasValue)
    {
      query = query.Where(x => x.Type == payload.Type.Value);
    }
    if (payload.Category.HasValue)
    {
      query = query.Where(x => x.Category == payload.Category.Value);
    }

    long total = await query.LongCountAsync(cancellationToken);

    if (payload.Limit < 1)
    {
      return new SearchResults<MoveDto>(total);
    }

    IOrderedQueryable<MoveEntity>? ordered = null;
    foreach (SortOption<MoveSort> sort in payload.Sort)
    {
      switch (sort.Field)
      {
        case MoveSort.Accuracy:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.Accuracy.HasValue).ThenByDescending(x => x.Accuracy)
              : query.OrderByDescending(x => x.Accuracy.HasValue).ThenBy(x => x.Accuracy))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.Accuracy.HasValue).ThenByDescending(x => x.Accuracy)
              : ordered.ThenByDescending(x => x.Accuracy.HasValue).ThenBy(x => x.Accuracy));
          break;
        case MoveSort.CreatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.CreatedOn) : ordered.ThenBy(x => x.CreatedOn));
          break;
        case MoveSort.Key:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Key) : ordered.ThenBy(x => x.Key));
          break;
        case MoveSort.Name:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.Name ?? x.Key) : query.OrderBy(x => x.Name ?? x.Key))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.Name ?? x.Key));
          break;
        case MoveSort.Power:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.Power.HasValue).ThenByDescending(x => x.Power)
              : query.OrderByDescending(x => x.Power.HasValue).ThenBy(x => x.Power))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.Power.HasValue).ThenByDescending(x => x.Power)
              : ordered.ThenByDescending(x => x.Power.HasValue).ThenBy(x => x.Power));
          break;
        case MoveSort.PowerPoints:
          ordered = ordered is null
            ? (sort.Direction == SortDirection.Descending
              ? query.OrderByDescending(x => x.PowerPoints.HasValue).ThenByDescending(x => x.PowerPoints)
              : query.OrderByDescending(x => x.PowerPoints.HasValue).ThenBy(x => x.PowerPoints))
            : (sort.Direction == SortDirection.Descending
              ? ordered.ThenByDescending(x => x.PowerPoints.HasValue).ThenByDescending(x => x.PowerPoints)
              : ordered.ThenByDescending(x => x.PowerPoints.HasValue).ThenBy(x => x.PowerPoints));
          break;
        case MoveSort.UpdatedOn:
          ordered = (ordered is null)
            ? (sort.Direction == SortDirection.Descending ? query.OrderByDescending(x => x.UpdatedOn) : query.OrderBy(x => x.UpdatedOn))
            : (sort.Direction == SortDirection.Descending ? ordered.ThenByDescending(x => x.UpdatedOn) : ordered.ThenBy(x => x.UpdatedOn));
          break;
      }
    }
    query = ordered is null ? query.OrderBy(x => x.Name ?? x.Key) : ordered.ThenBy(x => x.MoveId);

    query = query.Skip(payload.Offset).Take(payload.Limit);

    MoveEntity[] entities = await query.ToArrayAsync(cancellationToken);
    IReadOnlyCollection<MoveDto> moves = await MapAsync(entities, cancellationToken);

    return new SearchResults<MoveDto>(moves, total);
  }

  private async Task<MoveDto> MapAsync(MoveEntity move, CancellationToken cancellationToken)
  {
    return (await MapAsync([move], cancellationToken)).Single();
  }
  private async Task<IReadOnlyCollection<MoveDto>> MapAsync(IEnumerable<MoveEntity> moves, CancellationToken cancellationToken)
  {
    IEnumerable<ActorId> actorIds = moves.SelectMany(move => move.GetActorIds());
    IReadOnlyDictionary<ActorId, Actor> actors = await _actors.FindAsync(actorIds, cancellationToken);
    Mapper mapper = new(actors);

    return moves.Select(mapper.ToMove).ToList().AsReadOnly();
  }
}
