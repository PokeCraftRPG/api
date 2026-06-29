using Logitar.CQRS;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities.Commands;

internal record CreateOrReplaceAbilityCommand(CreateOrReplaceAbilityPayload Payload, Guid? Id) : ICommand<CreateOrReplaceAbilityResult>;

internal class CreateOrReplaceAbilityCommandHandler : ICommandHandler<CreateOrReplaceAbilityCommand, CreateOrReplaceAbilityResult>
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public CreateOrReplaceAbilityCommandHandler(
    IAbilityRepository abilityRepository,
    IContext context,
    IPermissionService permissionService,
    IWorldRepository worldRepository)
  {
    _abilityRepository = abilityRepository;
    _context = context;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<CreateOrReplaceAbilityResult> HandleAsync(CreateOrReplaceAbilityCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityPayload payload = command.Payload;
    payload.Validate();

    Ability? ability = null;
    if (command.Id.HasValue)
    {
      ability = await _abilityRepository.LoadAsync(command.Id.Value, cancellationToken);
    }

    bool created = false;
    if (ability is null)
    {
      World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
        ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
      await _permissionService.CheckAsync(Actions.CreateAbility, world, cancellationToken);

      ability = new Ability(world, payload.Key, _context.UserId, command.Id, payload.Name, payload.Description);
      _abilityRepository.Add(ability);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

      AbilityUpdated record = ability.Update(payload.Key, payload.Name, payload.Description, _context.UserId);
      _abilityRepository.Update(ability, record);
    }

    await _abilityRepository.EnsureUnicityAsync(ability, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    AbilityModel model = await _abilityRepository.ReadAsync(ability, cancellationToken);
    return new CreateOrReplaceAbilityResult(model, created);
  }
}
