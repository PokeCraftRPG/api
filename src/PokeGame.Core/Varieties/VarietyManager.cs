using PokeGame.Core.Moves;
using PokeGame.Core.Varieties.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public interface IVarietyManager
{
  Task<Variety> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<MoveId, int?>> FindMovesAsync(
    IEnumerable<VarietyMovePayload> payloads,
    string propertyName,
    CancellationToken cancellationToken = default);
}

internal class VarietyManager : IVarietyManager
{
  private readonly IContext _context;
  private readonly IMoveQuerier _moveQuerier;
  private readonly IVarietyQuerier _varietyQuerier;
  private readonly IVarietyRepository _varietyRepository;

  public VarietyManager(
    IContext context,
    IMoveQuerier moveQuerier,
    IVarietyQuerier varietyQuerier,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _moveQuerier = moveQuerier;
    _varietyQuerier = varietyQuerier;
    _varietyRepository = varietyRepository;
  }

  public async Task<Variety> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    if (Guid.TryParse(idOrKey, out Guid id))
    {
      VarietyId varietyId = new(worldId, id);
      Variety? variety = await _varietyRepository.LoadAsync(varietyId, cancellationToken);
      if (variety is not null)
      {
        return variety;
      }
    }

    VarietyId? foundId = await _varietyQuerier.FindIdAsync(idOrKey, cancellationToken);
    if (!foundId.HasValue)
    {
      throw new VarietyNotFoundException(worldId, idOrKey, propertyName);
    }

    return await _varietyRepository.LoadAsync(foundId.Value, cancellationToken)
      ?? throw new InvalidOperationException($"The variety 'Id={foundId}' was not loaded.");
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
