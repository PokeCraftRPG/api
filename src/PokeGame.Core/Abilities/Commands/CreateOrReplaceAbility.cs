using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Abilities.Commands;

internal record CreateOrReplaceAbilityCommand(CreateOrReplaceAbilityPayload Payload, Guid? Id) : ICommand<CreateOrReplaceAbilityResult>;

internal class CreateOrReplaceAbilityCommandHandler : ICommandHandler<CreateOrReplaceAbilityCommand, CreateOrReplaceAbilityResult>
{
  private readonly IAbilityManager _abilityManager;
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;

  public CreateOrReplaceAbilityCommandHandler(
    IAbilityManager abilityManager,
    IAbilityQuerier abilityQuerier,
    IAbilityRepository abilityRepository,
    IContext context,
    IPermissionService permissionService)
  {
    _abilityManager = abilityManager;
    _abilityQuerier = abilityQuerier;
    _abilityRepository = abilityRepository;
    _context = context;
    _permissionService = permissionService;
  }

  public async Task<CreateOrReplaceAbilityResult> HandleAsync(CreateOrReplaceAbilityCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityPayload payload = command.Payload;
    payload.Validate();

    AbilityId abilityId = AbilityId.NewId(_context.WorldId);
    Ability? ability = null;
    if (command.Id.HasValue)
    {
      abilityId = new AbilityId(abilityId.WorldId, command.Id.Value);
      ability = await _abilityRepository.LoadAsync(abilityId, cancellationToken);
    }

    Slug key = new(payload.Key);
    ActorId? actorId = _context.ActorId;

    bool created = false;
    if (ability is null)
    {
      await _permissionService.CheckAsync(Actions.CreateAbility, cancellationToken);

      ability = new Ability(abilityId, key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

      ability.SetKey(key, actorId);
    }

    ability.Rename(Name.TryCreate(payload.Name), actorId);
    ability.Describe(Description.TryCreate(payload.Description), actorId);

    await _abilityManager.EnsureUnicityAsync(ability, cancellationToken);
    await _abilityRepository.SaveAsync(ability, cancellationToken);

    AbilityModel model = await _abilityQuerier.FindAsync(ability, cancellationToken);
    return new CreateOrReplaceAbilityResult(model, created);
  }
}
