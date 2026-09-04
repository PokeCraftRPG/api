using Logitar.EventSourcing;
using PokeGame.Core.Varieties.Events;

namespace PokeGame.Core.Varieties;

public interface IVarietyManager
{
  Task EnsureUnicityAsync(Variety variety, CancellationToken cancellationToken = default);
}

internal class VarietyManager : IVarietyManager
{
  private readonly IVarietyQuerier _varietyQuerier;

  public VarietyManager(IVarietyQuerier varietyQuerier)
  {
    _varietyQuerier = varietyQuerier;
  }

  public async Task EnsureUnicityAsync(Variety variety, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in variety.Changes)
    {
      if (change is VarietyCreated created)
      {
        key = created.Key;
      }
      else if (change is VarietyKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      VarietyId? varietyId = await _varietyQuerier.GetIdAsync(key, cancellationToken);
      if (varietyId.HasValue && !varietyId.Value.Equals(variety.Id))
      {
        throw new KeyAlreadyUsedException(variety, varietyId.Value.EntityId, variety.Key, nameof(variety.Key));
      }
    }
  }
}
