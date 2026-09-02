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

    AbilityId? abilityId = null;
    Ability? ability = null;
    if (command.Id.HasValue)
    {
      abilityId = new(_context.WorldId, command.Id.Value);
      ability = await _abilityRepository.LoadAsync(abilityId.Value, cancellationToken);
    }

    ActorId? actorId = _context.ActorId;
    Key key = new(payload.Key);

    bool created = false;
    if (ability is null)
    {
      await _permissionService.CheckAsync(Actions.CreateAbility, cancellationToken);

      ability = new Ability(abilityId ?? AbilityId.NewId(_context.WorldId), key, actorId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

      ability.SetKey(key, actorId);
    }

    ability.Update(Name.TryCreate(payload.Name), Summary.TryCreate(payload.Summary), Content.TryCreate(payload.Content), actorId);

    await _abilityManager.EnsureUnicityAsync(ability, cancellationToken);
    await _abilityRepository.SaveAsync(ability, cancellationToken);

    AbilityDto dto = await _abilityQuerier.ReadAsync(ability, cancellationToken);
    return new CreateOrReplaceAbilityResult(dto, created);
  }
}
