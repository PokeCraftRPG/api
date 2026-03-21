using Logitar.CQRS;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities.Commands;

internal record CreateOrReplaceAbilityCommand(CreateOrReplaceAbilityPayload Payload, Guid? Id) : ICommand<CreateOrReplaceAbilityResult>;

internal class CreateOrReplaceAbilityCommandHandler : ICommandHandler<CreateOrReplaceAbilityCommand, CreateOrReplaceAbilityResult>
{
  private readonly IAbilityQuerier _abilityQuerier;
  private readonly IAbilityRepository _abilityRepository;
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IStorageService _storageService;

  public CreateOrReplaceAbilityCommandHandler(
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

  public async Task<CreateOrReplaceAbilityResult> HandleAsync(CreateOrReplaceAbilityCommand command, CancellationToken cancellationToken)
  {
    CreateOrReplaceAbilityPayload payload = command.Payload;
    payload.Validate();

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    AbilityId abilityId = AbilityId.NewId(worldId);
    Ability? ability = null;
    if (command.Id.HasValue)
    {
      abilityId = new(worldId, command.Id.Value);
      ability = await _abilityRepository.LoadAsync(abilityId, cancellationToken);
    }

    Name name = new(payload.Name);

    bool created = false;
    if (ability is null)
    {
      await _permissionService.CheckAsync(Actions.CreateAbility, cancellationToken);

      ability = new(name, userId, abilityId);
      created = true;
    }
    else
    {
      await _permissionService.CheckAsync(Actions.Update, ability, cancellationToken);

      ability.Name = name;
    }

    ability.Description = Description.TryCreate(payload.Description);

    ability.Url = Url.TryCreate(payload.Url);
    ability.Notes = Notes.TryCreate(payload.Notes);

    ability.Update(userId);

    await _storageService.ExecuteWithQuotaAsync(
      ability,
      async () => await _abilityRepository.SaveAsync(ability, cancellationToken),
      cancellationToken);

    AbilityModel model = await _abilityQuerier.ReadAsync(ability, cancellationToken);
    return new CreateOrReplaceAbilityResult(model, created);
  }
}
