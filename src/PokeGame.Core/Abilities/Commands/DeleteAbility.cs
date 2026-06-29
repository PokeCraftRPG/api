using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Abilities.Commands;

internal record DeleteAbilityCommand(Guid Id) : ICommand<AbilityModel?>;

internal class DeleteAbilityCommandHandler : ICommandHandler<DeleteAbilityCommand, AbilityModel?>
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;

  public DeleteAbilityCommandHandler(IAbilityRepository abilityRepository, IContext context, IPermissionService permissionService)
  {
    _abilityRepository = abilityRepository;
    _context = context;
    _permissionService = permissionService;
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
