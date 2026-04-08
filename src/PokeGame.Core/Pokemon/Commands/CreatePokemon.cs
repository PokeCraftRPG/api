using Logitar.CQRS;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

internal record CreatePokemonCommand(CreatePokemonPayload Payload) : ICommand<PokemonModel>;

internal class CreatePokemonCommandHandler : ICommandHandler<CreatePokemonCommand, PokemonModel>
{
  private readonly IContext _context;
  private readonly IFormManager _formManager;
  private readonly IPermissionService _permissionService;
  private readonly IPokemonQuerier _pokemonQuerier;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IPokemonRandomizer _randomizer = PokemonRandomizer.Instance;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IStorageService _storageService;
  private readonly IVarietyRepository _varietyRepository;

  public CreatePokemonCommandHandler(
    IContext context,
    IFormManager formManager,
    IPermissionService permissionService,
    IPokemonQuerier pokemonQuerier,
    IPokemonRepository pokemonRepository,
    ISpeciesRepository speciesRepository,
    IStorageService storageService,
    IVarietyRepository varietyRepository)
  {
    _context = context;
    _formManager = formManager;
    _permissionService = permissionService;
    _pokemonQuerier = pokemonQuerier;
    _pokemonRepository = pokemonRepository;
    _speciesRepository = speciesRepository;
    _storageService = storageService;
    _varietyRepository = varietyRepository;
  }

  public async Task<PokemonModel> HandleAsync(CreatePokemonCommand command, CancellationToken cancellationToken)
  {
    CreatePokemonPayload payload = command.Payload;
    payload.Validate();

    await _permissionService.CheckAsync(Actions.CreatePokemon, cancellationToken);

    UserId userId = _context.UserId;
    WorldId worldId = _context.WorldId;

    SpecimenId specimenId = SpecimenId.NewId(worldId);
    Specimen? specimen;
    if (payload.Id.HasValue)
    {
      specimenId = new SpecimenId(worldId, payload.Id.Value);
      specimen = await _pokemonRepository.LoadAsync(specimenId, cancellationToken);
      if (specimen is not null)
      {
        throw new PropertyConflictException<Guid>(specimen, payload.Id.Value, payload.Id.Value, nameof(payload.Id));
      }
    }

    Form form = await _formManager.FindAsync(payload.Form, nameof(payload.Form), cancellationToken);
    Variety variety = await _varietyRepository.LoadAsync(form.VarietyId, cancellationToken)
      ?? throw new InvalidOperationException($"The variety 'Id={form.VarietyId}' was not loaded.");
    SpeciesAggregate species = await _speciesRepository.LoadAsync(variety.SpeciesId, cancellationToken)
      ?? throw new InvalidOperationException($"The species 'Id={variety.SpeciesId}' was not loaded.");

    Slug? key = Slug.TryCreate(payload.Key);
    PokemonGender? gender = payload.Gender ?? _randomizer.Gender(variety.GenderRatio);
    PokemonSize size = payload.Size is null ? _randomizer.PokemonSize() : new(payload.Size);
    AbilitySlot abilitySlot = payload.AbilitySlot ?? _randomizer.AbilitySlot(form.Abilities);
    PokemonNature nature = string.IsNullOrWhiteSpace(payload.Nature) ? _randomizer.PokemonNature() : PokemonNatures.Instance.Find(payload.Nature);
    EggCycles? eggCycles = payload.EggCycles.HasValue ? new(payload.EggCycles.Value) : null;
    IndividualValues individualValues = payload.IndividualValues is null ? _randomizer.IndividualValues() : new(payload.IndividualValues);
    EffortValues? effortValues = payload.EffortValues is null ? null : new(payload.EffortValues);
    Friendship? friendship = payload.Friendship.HasValue ? new(payload.Friendship.Value) : null;
    specimen = new(species, variety, form, key, gender, payload.IsShiny, payload.TeraType, size, abilitySlot, nature, eggCycles,
      payload.Experience, individualValues, effortValues, payload.Vitality, payload.Stamina, friendship, userId, specimenId);

    specimen.Nickname(Name.TryCreate(payload.Name), userId);

    specimen.Sprite = Url.TryCreate(payload.Sprite);
    specimen.Url = Url.TryCreate(payload.Url);
    specimen.Notes = Notes.TryCreate(payload.Notes);

    specimen.Update(userId);

    await _pokemonQuerier.EnsureUnicityAsync(specimen, cancellationToken);

    await _storageService.ExecuteWithQuotaAsync(
      specimen,
      async () => await _pokemonRepository.SaveAsync(specimen, cancellationToken),
      cancellationToken);

    return await _pokemonQuerier.ReadAsync(specimen, cancellationToken);
  }
}
