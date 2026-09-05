using Logitar.EventSourcing;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Events;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public interface IFormManager
{
  Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken = default);
  Task<FormAbilities> ResolveAbilitiesAsync(FormAbilitiesPayload payload, string propertyName, CancellationToken cancellationToken = default);
}

internal class FormManager : IFormManager
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IFormQuerier _formQuerier;

  public FormManager(IAbilityRepository abilityRepository, IContext context, IFormQuerier formQuerier)
  {
    _abilityRepository = abilityRepository;
    _context = context;
    _formQuerier = formQuerier;
  }

  public async Task EnsureUnicityAsync(Form form, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in form.Changes)
    {
      if (change is FormCreated created)
      {
        key = created.Key;
      }
      else if (change is FormKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      FormId? formId = await _formQuerier.GetIdAsync(key, cancellationToken);
      if (formId.HasValue && !formId.Value.Equals(form.Id))
      {
        throw new KeyAlreadyUsedException(form, formId.Value.EntityId, form.Key, nameof(form.Key));
      }
    }
  }

  public async Task<FormAbilities> ResolveAbilitiesAsync(FormAbilitiesPayload payload, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    HashSet<AbilityId> abilityIds = new(capacity: 3);
    abilityIds.Add(new AbilityId(worldId, payload.PrimaryId));
    if (payload.SecondaryId.HasValue)
    {
      abilityIds.Add(new AbilityId(worldId, payload.SecondaryId.Value));
    }
    if (payload.HiddenId.HasValue)
    {
      abilityIds.Add(new AbilityId(worldId, payload.HiddenId.Value));
    }
    Dictionary<AbilityId, Ability> abilitiesById = (await _abilityRepository.LoadAsync(abilityIds, cancellationToken)).ToDictionary(x => x.Id, x => x);

    AbilityId abilityId = new(worldId, payload.PrimaryId);
    Ability primary = abilitiesById.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.PrimaryId)}");
    Ability? secondary = null;
    Ability? hidden = null;
    if (payload.SecondaryId.HasValue)
    {
      abilityId = new AbilityId(worldId, payload.SecondaryId.Value);
      secondary = abilitiesById.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.SecondaryId)}");
    }
    if (payload.HiddenId.HasValue)
    {
      abilityId = new AbilityId(worldId, payload.HiddenId.Value);
      hidden = abilitiesById.GetValueOrDefault(abilityId) ?? throw new EntityNotFoundException(abilityId, $"{propertyName}.{nameof(payload.HiddenId)}");
    }
    return FormAbilities.From(primary, secondary, hidden);
  }
}
