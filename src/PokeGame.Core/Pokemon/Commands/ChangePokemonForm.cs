using Logitar.CQRS;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Commands;

internal record ChangePokemonFormCommand(Guid Id, ChangePokemonFormPayload Payload) : ICommand<PokemonModel?>;

internal class ChangePokemonFormCommandHandler : ICommandHandler<ChangePokemonFormCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IFormManager _formManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IStorageService _storageService;
  private readonly IVarietyRepository _varietyRepository;

  public ChangePokemonFormCommandHandler(
    IContext context,
    IFormManager formManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IStorageService storageService,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _formManager = formManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _storageService = storageService;
    _varietyRepository = varietyRepository;
  }

  public async Task<PokemonModel?> HandleAsync(ChangePokemonFormCommand command, CancellationToken cancellationToken)
  {
    ChangePokemonFormPayload payload = command.Payload;
    payload.Validate();

    SpecimenId specimenId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(specimenId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }

    await _permissionService.CheckAsync(Actions.ChangeForm, specimen, cancellationToken);

    Variety variety = await _varietyRepository.LoadAsync(specimen.VarietyId, cancellationToken)
      ?? throw new InvalidOperationException($"The variety 'Id={specimen.VarietyId}' was not loaded.");
    if (!variety.CanChangeForm)
    {
      throw new PokemonCannotChangeFormException(specimen);
    }

    Form form = await _formManager.FindAsync(payload.Form, nameof(payload.Form), cancellationToken);

    specimen.ChangeForm(form, _context.UserId);

    await _storageService.ExecuteWithQuotaAsync(
      specimen,
      async () => await _pokemonRepository.SaveAsync(specimen, cancellationToken),
      cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
