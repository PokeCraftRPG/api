using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Commands;

internal record CreateOrReplaceAbilityCommand(CreateOrReplaceAbilityPayload Payload, Guid? Id) : ICommand<CreateOrReplaceAbilityResult>;

internal class CreateOrReplaceAbilityCommandHandler : ICommandHandler<CreateOrReplaceAbilityCommand, CreateOrReplaceAbilityResult>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IAbilityRepository _abilityRepository;

  public CreateOrReplaceAbilityCommandHandler(IContext context, IPermissionService permissionService, IAbilityRepository abilityRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _abilityRepository = abilityRepository;
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
      await _permissionService.CheckAsync(Actions.CreateAbility, cancellationToken);

      ability = new Ability(_context.WorldId, payload.Key, _context.UserId, command.Id, payload.Name, payload.Description);
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
