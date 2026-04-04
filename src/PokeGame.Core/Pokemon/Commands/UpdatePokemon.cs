using Logitar.CQRS;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Pokemon.Commands;

internal record UpdatePokemonCommand(Guid Id, UpdatePokemonPayload Payload) : ICommand<PokemonModel?>;

internal class UpdatePokemonCommandHandler : ICommandHandler<UpdatePokemonCommand, PokemonModel?>
{
  private readonly IContext _context;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IStorageService _storageService;

  public UpdatePokemonCommandHandler(
    IContext context,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IStorageService storageService)
  {
    _context = context;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _storageService = storageService;
  }

  public async Task<PokemonModel?> HandleAsync(UpdatePokemonCommand command, CancellationToken cancellationToken)
  {
    UpdatePokemonPayload payload = command.Payload;
    payload.Validate();

    SpecimenId specimenId = new(_context.WorldId, command.Id);
    Specimen? specimen = await _pokemonRepository.LoadAsync(specimenId, cancellationToken);
    if (specimen is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Update, specimen, cancellationToken);

    UserId userId = _context.UserId;

    if (!string.IsNullOrWhiteSpace(payload.Key))
    {
      specimen.SetKey(new Slug(payload.Key), userId);
    }
    if (payload.Name is not null)
    {
      specimen.Nickname(Name.TryCreate(payload.Name.Value), userId);
    }

    if (payload.Sprite is not null)
    {
      specimen.Sprite = Url.TryCreate(payload.Sprite.Value);
    }
    if (payload.Url is not null)
    {
      specimen.Url = Url.TryCreate(payload.Url.Value);
    }
    if (payload.Notes is not null)
    {
      specimen.Notes = Notes.TryCreate(payload.Notes.Value);
    }

    specimen.Update(userId);

    await _pokemonQuerier.EnsureUnicityAsync(specimen, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      specimen,
      async () => await _pokemonRepository.SaveAsync(specimen, cancellationToken),
      cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
