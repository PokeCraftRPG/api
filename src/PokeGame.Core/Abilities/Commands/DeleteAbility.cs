using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Commands;

internal record DeleteAbilityCommand(Guid Id) : ICommand<AbilityModel?>;

internal class DeleteAbilityCommandHandler : ICommandHandler<DeleteAbilityCommand, AbilityModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IAbilityRepository _abilityRepository;

  public DeleteAbilityCommandHandler(IContext context, IPermissionService permissionService, IAbilityRepository abilityRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _abilityRepository = abilityRepository;
  }

  public async Task<AbilityModel?> HandleAsync(DeleteAbilityCommand command, CancellationToken cancellationToken)
  {
    Ability? ability = await _abilityRepository.LoadAsync(command.Id, cancellationToken);
    if (ability is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Delete, ability, cancellationToken);

    AbilityModel model = await _abilityRepository.ReadAsync(ability, cancellationToken);

    _abilityRepository.Remove(ability);

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
