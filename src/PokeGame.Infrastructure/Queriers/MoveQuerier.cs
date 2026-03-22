using Krakenar.Contracts.Actors;
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
  private readonly IActorService _actors;
  private readonly IContext _context;
  private readonly DbSet<MoveEntity> _moves;

  public MoveQuerier(IActorService actors, IContext context, PokemonContext pokemon)
  {
    _actors = actors;
    _context = context;
    _moves = pokemon.Moves;
  }

  public async Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken)
  {
    string? streamId = await _moves.Where(x => x.World!.Id == move.WorldId.ToGuid() && x.Key == move.Key.Value)
      .Select(x => x.StreamId)
      .SingleOrDefaultAsync(cancellationToken);
    if (streamId is not null && streamId != move.Id.Value)
    {
      throw new PropertyConflictException<string>(move, new MoveId(streamId).EntityId, move.Key.Value, nameof(Move.Key));
    }
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
