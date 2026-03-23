using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Species.Commands;

internal record UpdateSpeciesCommand(Guid Id, UpdateSpeciesPayload Payload) : ICommand<SpeciesModel?>;

internal class UpdateSpeciesCommandHandler : ICommandHandler<UpdateSpeciesCommand, SpeciesModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IStorageService _storageService;

  public UpdateSpeciesCommandHandler(
    IContext context,
    IPermissionService permissionService,
    ISpeciesQuerier speciesQuerier,
    ISpeciesRepository speciesRepository,
    IStorageService storageService)
  {
    _context = context;
    _permissionService = permissionService;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
    _storageService = storageService;
  }

  public async Task<SpeciesModel?> HandleAsync(UpdateSpeciesCommand command, CancellationToken cancellationToken)
  {
    UpdateSpeciesPayload payload = command.Payload;
    payload.Validate();

    SpeciesId speciesId = new(_context.WorldId, command.Id);
    SpeciesAggregate? species = await _speciesRepository.LoadAsync(speciesId, cancellationToken);
    if (species is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, species, cancellationToken);

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      Slug key = new(payload.Key);
      species.SetKey(key, userId);
    }
    if (payload.Name is not null)
    {
      species.Name = Name.TryCreate(payload.Name.Value);
    }

    species.Update(userId);

    if (species.Changes.Any(change => change is SpeciesKeyChanged))
    {
      await _speciesQuerier.EnsureUnicityAsync(species, cancellationToken);
    }

    await _storageService.ExecuteWithQuotaAsync(
      species,
      async () => await _speciesRepository.SaveAsync(species, cancellationToken),
      cancellationToken);

    return await _speciesQuerier.ReadAsync(species, cancellationToken);
  }
}
