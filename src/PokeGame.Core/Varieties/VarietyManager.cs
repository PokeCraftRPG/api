using PokeGame.Core.Moves;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties;

public interface IVarietyManager
{
  Task<IReadOnlyDictionary<MoveId, int?>> FindMovesAsync(
    IEnumerable<VarietyMovePayload> payloads,
    string propertyName,
    CancellationToken cancellationToken = default);
}

internal class VarietyManager : IVarietyManager
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;

  public VarietyManager(IContext context, IMoveQuerier moveQuerier)
  {
    _context = context;
    _moveQuerier = moveQuerier;
  }

  public async Task<IReadOnlyDictionary<MoveId, int?>> FindMovesAsync(
    IEnumerable<VarietyMovePayload> payloads,
    string propertyName,
    CancellationToken cancellationToken)
  {
    int capacity = payloads.Count();
    Dictionary<MoveId, int?> varietyMoves = new(capacity);

    if (capacity > 0)
    {
      IReadOnlyCollection<MoveKey> allKeys = await _moveQuerier.ListKeysAsync(cancellationToken);
      Dictionary<Guid, MoveId> ids = new(capacity: allKeys.Count);
      Dictionary<string, MoveId> keys = new(capacity: allKeys.Count);
      foreach (MoveKey key in allKeys)
      {
        ids[key.Id] = key.MoveId;
        keys[key.Key] = key.MoveId;
      }

      List<string> missing = new(capacity);
      foreach (VarietyMovePayload payload in payloads)
      {
        string move = Slug.Normalize(payload.Move);
        if ((Guid.TryParse(move, out Guid id) && ids.TryGetValue(id, out MoveId moveId)) || keys.TryGetValue(move, out moveId))
        {
          varietyMoves[moveId] = payload.Level;
        }
        else
        {
          missing.Add(payload.Move);
        }
      }

      if (missing.Count > 0)
      {
        throw new MovesNotFoundException(_context.WorldId, missing, propertyName);
      }
    }

    return varietyMoves.AsReadOnly();
  }
}
