using Logitar.CQRS;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Commands;

internal record CreatePokemonCommand(CreatePokemonPayload Payload) : ICommand<PokemonDto>;

internal class CreatePokemonCommandHandler : ICommandHandler<CreatePokemonCommand, PokemonDto>
{
  private readonly IContext _context;
  private readonly IFormRepository _formRepository;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonManager _pokemonManager;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IPokemonRandomizer _randomizer;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  public CreatePokemonCommandHandler(
    IContext context,
    IFormRepository formRepository,
    IPermissionService permissionService,
    IPokemonManager pokemonManager,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    IPokemonRandomizer randomizer,
    ISpeciesRepository speciesRepository,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _formRepository = formRepository;
    _permissionService = permissionService;
    _pokemonManager = pokemonManager;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _randomizer = randomizer;
    _speciesRepository = speciesRepository;
    _varietyRepository = varietyRepository;
  }

  public async Task<PokemonDto> HandleAsync(CreatePokemonCommand command, CancellationToken cancellationToken)
  {
    CreatePokemonPayload payload = command.Payload;
    payload.Validate();

    await _permissionService.CheckAsync(Actions.CreatePokemon, cancellationToken);

    FormId formId = new(_context.WorldId, payload.FormId);
    Form form = await _formRepository.LoadAsync(formId, cancellationToken) ?? throw new EntityNotFoundException(formId, nameof(payload.FormId));
    Variety variety = await _varietyRepository.LoadAsync(form.VarietyId, cancellationToken)
      ?? throw new InvalidOperationException($"The variety 'Id={form.VarietyId}' was not loaded.");
    PokemonSpecies species = await _speciesRepository.LoadAsync(variety.SpeciesId, cancellationToken)
      ?? throw new InvalidOperationException($"The species 'Id={variety.SpeciesId}' was not loaded.");

    PokemonId pokemonId = PokemonId.NewId(_context.WorldId);
    Key? key = string.IsNullOrWhiteSpace(payload.Key) ? null : new(payload.Key);
    PokemonSize? size = payload.Size.HasValue ? new(payload.Size.Value) : null;
    PokemonNature? nature = string.IsNullOrWhiteSpace(payload.Nature) ? null : PokemonNatures.Find(payload.Nature);
    IndividualValues? individualValues = payload.IndividualValues is null ? null : IndividualValues.From(payload.IndividualValues);

    Specimen specimen = new(
      _randomizer,
      pokemonId,
      species,
      variety,
      form,
      key,
      payload.Gender,
      payload.IsShiny,
      payload.TeraType,
      payload.AbilitySlot,
      size,
      nature,
      payload.EggCycles,
      payload.Experience,
      individualValues,
      _context.ActorId);

    await _pokemonManager.EnsureUnicityAsync(specimen, cancellationToken);
    await _pokemonRepository.SaveAsync(specimen, cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
