using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Abilities.Commands;

internal record UpdateAbilityCommand(Guid Id, UpdateAbilityPayload Payload) : ICommand<AbilityModel?>;

internal class UpdateAbilityCommandHandler : ICommandHandler<UpdateAbilityCommand, AbilityModel?>
{
  private readonly IAbilityManager _abilityManager;
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;

  public UpdateAbilityCommandHandler(
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

  public async Task<AbilityModel?> HandleAsync(UpdateAbilityCommand command, CancellationToken cancellationToken)
  {
    UpdateAbilityPayload payload = command.Payload;
    payload.Validate();

    AbilityId abilityId = new(_context.WorldId, command.Id);
    Ability? ability = await _abilityRepository.LoadAsync(abilityId, cancellationToken);
    if (ability is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

    ActorId? actorId = _context.ActorId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      ability.SetKey(new Slug(payload.Key), actorId);
    }
    if (payload.Name is not null)
    {
      ability.Rename(Name.TryCreate(payload.Name.Value), actorId);
    }
    if (payload.Description is not null)
    {
      ability.Describe(Description.TryCreate(payload.Description.Value), actorId);
    }

    await _abilityManager.EnsureUnicityAsync(ability, cancellationToken);
    await _abilityRepository.SaveAsync(ability, cancellationToken);

    return await _abilityQuerier.FindAsync(ability, cancellationToken);
  }
}
