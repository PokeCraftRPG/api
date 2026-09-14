using Logitar.CQRS;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record ChangePokemonFormCommand(Guid PokemonId, Guid FormId) : ICommand<PokemonDto?>;

internal class ChangePokemonFormCommandHandler : ICommandHandler<ChangePokemonFormCommand, PokemonDto?>
{
  private readonly IContext _context;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;

  public ChangePokemonFormCommandHandler(
    IContext context,
    IFormRepository formRepository,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository)
  {
    _context = context;
    _formRepository = formRepository;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
  }

  public async Task<PokemonDto?> HandleAsync(ChangePokemonFormCommand command, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    PokemonId pokemonId = new(worldId, command.PokemonId);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.ChangeForm, specimen, cancellationToken);

    FormId formId = new(worldId, command.FormId);
    Form form = await _formRepository.LoadAsync(formId, cancellationToken) ?? throw new EntityNotFoundException(formId, nameof(command.FormId));

    specimen.ChangeForm(form, _context.ActorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
