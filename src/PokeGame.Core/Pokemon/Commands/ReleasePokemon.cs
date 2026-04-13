using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon.Commands;

internal record ReleasePokemonCommand(Guid Id) : ICommand<PokemonModel?>;

internal class ReleasePokemonCommandHandler : ICommandHandler<ReleasePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;

  public ReleasePokemonCommandHandler(IContext context, IPermissionService permissionService, IPokemonQuerier pokemonQuerier, IPokemonRepository pokemonRepository)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
  }

  public async Task<PokemonModel?> HandleAsync(ReleasePokemonCommand command, CancellationToken cancellationToken)
  {
    SpecimenId specimenId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(specimenId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Release, specimen, cancellationToken);

    specimen.Release(_context.UserId);

    // TODO(fpion): update storage

    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
