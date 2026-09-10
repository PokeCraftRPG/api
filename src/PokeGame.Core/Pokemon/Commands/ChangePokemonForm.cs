using Logitar.CQRS;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;

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
    PokemonId pokemonId = new(_context.WorldId, command.PokemonId);
    Specimen? specimen = await _pokemonRepository.LoadAsync(pokemonId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, specimen, cancellationToken);

    FormId formId = new(specimen.WorldId, command.FormId);
    Form form = await _formRepository.LoadAsync(formId, cancellationToken) ?? throw new EntityNotFoundException(formId, nameof(command.FormId));

    specimen.ChangeForm(form, _context.ActorId);

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
