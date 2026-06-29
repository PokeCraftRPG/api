using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Abilities.Events;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Commands;

internal record UpdateAbilityCommand(Guid Id, UpdateAbilityPayload Payload) : ICommand<AbilityModel?>;

internal class UpdateAbilityCommandHandler : ICommandHandler<UpdateAbilityCommand, AbilityModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IAbilityRepository _abilityRepository;

  public UpdateAbilityCommandHandler(IContext context, IPermissionService permissionService, IAbilityRepository abilityRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _abilityRepository = abilityRepository;
  }

  public async Task<AbilityModel?> HandleAsync(UpdateAbilityCommand command, CancellationToken cancellationToken)
  {
    UpdateAbilityPayload payload = command.Payload;
    payload.Validate();

    Ability? ability = await _abilityRepository.LoadAsync(command.Id, cancellationToken);
    if (ability is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

    AbilityUpdated record = ability.Update(
      string.IsNullOrWhiteSpace(payload.Key) ? ability.Key : payload.Key,
      payload.Name is null ? ability.Name : payload.Name.Value,
      payload.Description is null ? ability.Description : payload.Description.Value,
      _context.UserId);
    _abilityRepository.Update(ability, record);

    await _abilityRepository.EnsureUnicityAsync(ability, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    return await _abilityRepository.ReadAsync(ability, cancellationToken);
  }
}
