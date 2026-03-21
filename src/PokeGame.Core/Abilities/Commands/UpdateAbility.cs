using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Abilities.Commands;

internal record UpdateAbilityCommand(Guid Id, UpdateAbilityPayload Payload) : ICommand<AbilityModel?>;

internal class UpdateAbilityCommandHandler : ICommandHandler<UpdateAbilityCommand, AbilityModel?>
{
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public UpdateAbilityCommandHandler(
    IAbilityQuerier abilityQuerier,
    IAbilityRepository abilityRepository,
    IContext context,
    IPermissionService permissionService,
    IStorageService storageService)
  {
    _abilityQuerier = abilityQuerier;
    _abilityRepository = abilityRepository;
    _context = context;
    _permissionService = permissionService;
    _storageService = storageService;
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

    if (!string.IsNullOrWhiteSpace(payload.Name))
    {
      ability.Name = new Name(payload.Name);
    }
    if (payload.Description is not null)
    {
      ability.Description = Description.TryCreate(payload.Description.Value);
    }

    if (payload.Url is not null)
    {
      ability.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      ability.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    ability.Update(_context.UserId);

    await _storageService.ExecuteWithQuotaAsync(
      ability,
      async () => await _abilityRepository.SaveAsync(ability, cancellationToken),
      cancellationToken);

    return await _abilityQuerier.ReadAsync(ability, cancellationToken);
  }
}
