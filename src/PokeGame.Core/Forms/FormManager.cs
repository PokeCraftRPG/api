using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms;

public interface IFormManager
{
  Task<Abilities> FindAbilitiesAsync(AbilitiesPayload payload, string propertyName, CancellationToken cancellationToken = default);
  Task<Form> FindAsync(string form, string propertyName, CancellationToken cancellationToken = default);
}

internal class FormManager : IFormManager
{
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IContext _context;

  public FormManager(IAbilityQuerier abilityQuerier, IContext context)
  {
    _abilityQuerier = abilityQuerier;
    _context = context;
  }

  public async Task<Abilities> FindAbilitiesAsync(AbilitiesPayload payload, string propertyName, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<AbilityKey> allKeys = await _abilityQuerier.ListKeysAsync(cancellationToken);
    Dictionary<Guid, AbilityId> ids = new(capacity: allKeys.Count);
    Dictionary<string, AbilityId> keys = new(capacity: allKeys.Count);
    foreach (AbilityKey key in allKeys)
    {
      ids[key.Id] = key.AbilityId;
      keys[key.Key] = key.AbilityId;
    }

    return new Abilities(
      FindAbilityId(payload.Primary, ids, keys, $"{propertyName}.{nameof(payload.Primary)}"),
      payload.Secondary is null ? null : FindAbilityId(payload.Secondary, ids, keys, $"{propertyName}.{nameof(payload.Secondary)}"),
      payload.Hidden is null ? null : FindAbilityId(payload.Hidden, ids, keys, $"{propertyName}.{nameof(payload.Hidden)}"));
  }
  private AbilityId FindAbilityId(string idOrKey, Dictionary<Guid, AbilityId> ids, Dictionary<string, AbilityId> keys, string propertyName)
  {
    string normalized = Slug.Normalize(idOrKey);
    if ((Guid.TryParse(normalized, out Guid id) && ids.TryGetValue(id, out AbilityId abilityId)) || keys.TryGetValue(normalized, out abilityId))
    {
      return abilityId;
    }
    throw new AbilityNotFoundException(_context.WorldId, idOrKey, propertyName);
  }

  public async Task<Form> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    throw new NotImplementedException(); // TODO(fpion): implement
  }
}
