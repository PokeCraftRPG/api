using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Seo;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IPokemonService _pokemonService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _ability = null!;
  private Form _form = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;

  public PokemonIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _ability = AbilityBuilder.Overgrow(Faker, Context.World);
    await _abilityRepository.SaveAsync(_ability);

    _species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Bulbasaur(Faker, _species, Context.World);
    await _varietyRepository.SaveAsync(_variety);

    _form = FormBuilder.Bulbasaur(Faker, _variety, _ability, Context.World);
    await _formRepository.SaveAsync(_form);
  }

  [Fact(DisplayName = "It should create a new Pokémon.")]
  public async Task Given_ValidPayload_When_Create_Then_Created()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = "starter-bulbasaur",
      Gender = Gender.Male,
      IsShiny = false,
      TeraType = PokemonType.Grass,
      AbilitySlot = AbilitySlot.Primary,
      Size = 128,
      Nature = "Hardy",
      Experience = 0,
      IndividualValues = new IndividualValuesDto
      {
        HP = 10,
        Attack = 11,
        Defense = 12,
        SpecialAttack = 13,
        SpecialDefense = 14,
        Speed = 15
      }
    };

    PokemonDto pokemon = await _pokemonService.CreateAsync(payload);
    Assert.NotEqual(Guid.Empty, pokemon.Id);
    Assert.Equal(1, pokemon.Version);
    Assert.Equal(Actor, pokemon.CreatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(pokemon.CreatedBy, pokemon.UpdatedBy);

    Assert.Equal(_form.EntityId, pokemon.Form.Id);
    Assert.Equal(SlugHelper.Format(payload.Key), pokemon.Key);
    Assert.Equal(Gender.Male, pokemon.Gender);
    Assert.False(pokemon.IsShiny);
    Assert.Equal(PokemonType.Grass, pokemon.TeraType);
    Assert.Equal(AbilitySlot.Primary, pokemon.AbilitySlot);
    Assert.Equal((byte)128, pokemon.Size);
    Assert.Equal(SizeCategory.Medium, pokemon.SizeCategory);
    Assert.Equal("Hardy", pokemon.Nature);
    Assert.Equal((byte)0, pokemon.EggCycles);
    Assert.False(pokemon.IsEgg);
    Assert.Equal(0, pokemon.Experience);
    Assert.Equal((byte)1, pokemon.Level);
    Assert.Equal((byte)10, pokemon.IndividualValues.HP);
    Assert.Equal((byte)11, pokemon.IndividualValues.Attack);
    Assert.Equal((byte)12, pokemon.IndividualValues.Defense);
    Assert.Equal((byte)13, pokemon.IndividualValues.SpecialAttack);
    Assert.Equal((byte)14, pokemon.IndividualValues.SpecialDefense);
    Assert.Equal((byte)15, pokemon.IndividualValues.Speed);
    Assert.True(pokemon.Vitality > 0);
    Assert.Equal(pokemon.Vitality, pokemon.Stamina);
    Assert.Null(pokemon.HeldItem);
    Assert.Null(pokemon.Sprite);

    PokemonDto? read = await _pokemonService.ReadAsync(pokemon.Id);
    Assert.NotNull(read);
    Assert.Equal(pokemon.Id, read.Id);
    Assert.Equal(pokemon.Key, read.Key);
  }

  [Fact(DisplayName = "It should read a Pokémon by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = "read-by-key"
    };
    PokemonDto created = await _pokemonService.CreateAsync(payload);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(key: created.Key);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Id, pokemon.Id);
    Assert.Equal(created.Key, pokemon.Key);
  }

  [Fact(DisplayName = "It should create a Pokémon egg.")]
  public async Task Given_EggCycles_When_Create_Then_EggCreated()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      EggCycles = 10
    };

    PokemonDto pokemon = await _pokemonService.CreateAsync(payload);
    Assert.Equal((byte)10, pokemon.EggCycles);
    Assert.True(pokemon.IsEgg);
    Assert.Equal(0, pokemon.Experience);
    Assert.Equal((byte)1, pokemon.Level);
  }

  [Fact(DisplayName = "It should return null when no Pokémon was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.ReadAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the form does not exist.")]
  public async Task Given_MissingForm_When_Create_Then_EntityNotFoundException()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = Guid.NewGuid()
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Form.EntityKind, exception.EntityKind);
    Assert.Equal(nameof(payload.FormId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = "unique-starter"
    };
    PokemonDto created = await _pokemonService.CreateAsync(payload);

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Specimen.EntityKind, exception.EntityKind);
    Assert.Equal(created.Id, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Specimen.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw InvalidEggCyclesException when egg cycles exceed the species maximum.")]
  public async Task Given_TooManyEggCycles_When_Create_Then_InvalidEggCyclesException()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      EggCycles = (byte)(_species.Eggs.Cycles + 1)
    };

    InvalidEggCyclesException exception = await Assert.ThrowsAsync<InvalidEggCyclesException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(payload.EggCycles, exception.AttemptedEggCycles);
    Assert.Equal(_species.Eggs.Cycles, exception.MaximumEggCycles);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonFormCategoryException when the form category is invalid.")]
  public async Task Given_MegaForm_When_Create_Then_InvalidPokemonFormCategoryException()
  {
    Form mega = new FormBuilder(Faker)
      .WithWorld(Context.World)
      .WithVariety(_variety)
      .WithAbilities(_ability)
      .WithCategory(FormCategory.Mega)
      .WithKey("mega-venusaur")
      .WithName("Mega Venusaur")
      .WithTypes(PokemonType.Grass, PokemonType.Poison)
      .WithBaseStatistics(80, 100, 123, 122, 120, 80)
      .WithYield(281, 0, 0, 0, 2, 1, 0)
      .WithSize(24, 1555)
      .Build();
    await _formRepository.SaveAsync(mega);

    CreatePokemonPayload payload = new()
    {
      FormId = mega.EntityId
    };

    InvalidPokemonFormCategoryException exception = await Assert.ThrowsAsync<InvalidPokemonFormCategoryException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(mega.EntityId, exception.FormId);
    Assert.Equal(FormCategory.Mega, exception.AttemptedCategory);
  }

  [Fact(DisplayName = "It should throw InvalidAbilitySlotException when the ability slot is unavailable.")]
  public async Task Given_HiddenAbilityMissing_When_Create_Then_InvalidAbilitySlotException()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      AbilitySlot = AbilitySlot.Hidden
    };

    InvalidAbilitySlotException exception = await Assert.ThrowsAsync<InvalidAbilitySlotException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(AbilitySlot.Hidden, exception.AttemptedSlot);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonGenderException when the gender is not allowed.")]
  public async Task Given_GenderlessVariety_When_Create_Then_InvalidPokemonGenderException()
  {
    Variety genderless = new VarietyBuilder(Faker)
      .WithWorld(Context.World)
      .WithSpecies(_species)
      .WithKey("genderless-bulb")
      .WithName("Genderless Bulbasaur")
      .WithIsDefault(false)
      .WithGenderRatio(null)
      .Build();
    await _varietyRepository.SaveAsync(genderless);

    Form form = new FormBuilder(Faker)
      .WithWorld(Context.World)
      .WithVariety(genderless)
      .WithAbilities(_ability)
      .WithKey("genderless-bulb-form")
      .WithName("Genderless Bulbasaur")
      .WithTypes(PokemonType.Grass, PokemonType.Poison)
      .WithBaseStatistics(45, 49, 49, 65, 65, 45)
      .WithYield(64, 0, 0, 0, 1, 0, 0)
      .WithSize(7, 69)
      .Build();
    await _formRepository.SaveAsync(form);

    CreatePokemonPayload payload = new()
    {
      FormId = form.EntityId,
      Gender = Gender.Male
    };

    InvalidPokemonGenderException exception = await Assert.ThrowsAsync<InvalidPokemonGenderException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Gender.Male, exception.AttemptedGender);
    Assert.Null(exception.FemaleRate);
  }

  [Fact(DisplayName = "It should throw ValidationException when egg cycles and experience are both set.")]
  public async Task Given_EggAndExperience_When_Create_Then_ValidationException()
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      EggCycles = 5,
      Experience = 100
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _pokemonService.CreateAsync(payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a Pokémon.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId
    };

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreatePokemon", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }
}
