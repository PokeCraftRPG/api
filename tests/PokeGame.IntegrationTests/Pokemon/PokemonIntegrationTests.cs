using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
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
  private readonly IAssetService _assetService;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
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
    _assetService = ServiceProvider.GetRequiredService<IAssetService>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
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
    Assert.Equal((byte)128, pokemon.Size.Scale);
    Assert.Equal(SizeCategory.Medium, pokemon.Size.Category);
    Assert.Equal("Hardy", pokemon.Nature.Name);
    Assert.Null(pokemon.Nature.IncreasedStatistic);
    Assert.Null(pokemon.Nature.DecreasedStatistic);
    Assert.Equal((byte)0, pokemon.EggCycles);
    Assert.Equal(0, pokemon.Experience);
    Assert.Equal(1, pokemon.Level);
    Assert.Equal((byte)10, pokemon.Statistics.HP.Individual);
    Assert.Equal((byte)11, pokemon.Statistics.Attack.Individual);
    Assert.Equal((byte)12, pokemon.Statistics.Defense.Individual);
    Assert.Equal((byte)13, pokemon.Statistics.SpecialAttack.Individual);
    Assert.Equal((byte)14, pokemon.Statistics.SpecialDefense.Individual);
    Assert.Equal((byte)15, pokemon.Statistics.Speed.Individual);
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
    Assert.Equal(0, pokemon.Experience);
    Assert.Equal(1, pokemon.Level);
  }

  [Fact(DisplayName = "It should return null when no Pokémon was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.ReadAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the form does not exist.")]
  public async Task Given_MissingForm_When_Create_Then_EntityNotFoundException()
  {
    Guid missingFormId = Guid.NewGuid();
    CreatePokemonPayload payload = new()
    {
      FormId = missingFormId
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.CreateAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingFormId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.FormId), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Specimen.EntityKind, exception.Data["EntityKind"]);
    Assert.NotEqual(created.Id, exception.Data["EntityId"]);
    Assert.NotEqual(Guid.Empty, exception.Data["EntityId"]);
    Assert.Equal(created.Id, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Specimen.Key), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.NotEqual(Guid.Empty, exception.Data["PokemonId"]);
    Assert.Equal(_species.EntityId, exception.Data["SpeciesId"]);
    Assert.Equal(_species.Eggs.Cycles, exception.Data["MaximumEggCycles"]);
    Assert.Equal(payload.EggCycles, exception.Data["AttemptedEggCycles"]);
    Assert.Equal(nameof(Specimen.EggCycles), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.NotEqual(Guid.Empty, exception.Data["PokemonId"]);
    Assert.Equal(mega.EntityId, exception.Data["FormId"]);
    Assert.Equal(FormCategory.Mega, exception.Data["AttemptedCategory"]);
    Assert.Equal(nameof(Specimen.FormId), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.NotEqual(Guid.Empty, exception.Data["PokemonId"]);
    Assert.Equal(_form.EntityId, exception.Data["FormId"]);
    Assert.Equal(AbilitySlot.Hidden, exception.Data["AttemptedSlot"]);
    Assert.Equal(nameof(Specimen.AbilitySlot), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.NotEqual(Guid.Empty, exception.Data["PokemonId"]);
    Assert.Equal(genderless.EntityId, exception.Data["VarietyId"]);
    Assert.Null(exception.Data["FemaleRate"]);
    Assert.Equal(Gender.Male, exception.Data["AttemptedGender"]);
    Assert.Equal(nameof(Specimen.Gender), exception.Data["PropertyName"]);
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
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("CreatePokemon", exception.Data["Action"]);
    Assert.Null(exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should return null when updating a Pokémon that does not exist.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.UpdateAsync(Guid.NewGuid(), new UpdatePokemonPayload()));
  }

  [Fact(DisplayName = "It should update an existing Pokémon.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    PokemonDto created = await CreatePokemonAsync("update-me");

    UpdatePokemonPayload payload = new()
    {
      Key = "updated-starter",
      Nickname = new Optional<string>(" Bulby "),
      Summary = new Optional<string>("  A loyal starter.  "),
      Content = new Optional<string>("   Always ready for battle.   ")
    };

    PokemonDto? pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Id, pokemon.Id);
    Assert.Equal(4, pokemon.Version);
    Assert.Equal(created.CreatedBy, pokemon.CreatedBy);
    Assert.Equal(created.CreatedOn, pokemon.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(SlugHelper.Format(payload.Key), pokemon.Key);
    Assert.Equal(payload.Nickname.Value?.Trim(), pokemon.Nickname);
    Assert.Equal(payload.Summary.Value?.Trim(), pokemon.Summary);
    Assert.Equal(payload.Content.Value?.Trim(), pokemon.Content);
  }

  [Fact(DisplayName = "It should update a Pokémon status.")]
  public async Task Given_Status_When_Update_Then_Updated()
  {
    PokemonDto created = await CreatePokemonAsync("status");

    UpdatePokemonPayload payload = new()
    {
      Vitality = 0,
      Stamina = 1,
      Condition = new Optional<StatusCondition?>(StatusCondition.Poison),
      Friendship = 200
    };

    PokemonDto? pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Equal(payload.Vitality, pokemon.Vitality);
    Assert.Equal(payload.Stamina, pokemon.Stamina);
    Assert.Equal(StatusCondition.Poison, pokemon.Condition);
    Assert.Equal(payload.Friendship, pokemon.Friendship);

    payload = new()
    {
      Condition = new Optional<StatusCondition?>(null)
    };
    pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Equal(0, pokemon.Vitality);
    Assert.Equal(1, pokemon.Stamina);
    Assert.Null(pokemon.Condition);
    Assert.Equal((byte)200, pokemon.Friendship);
  }

  [Fact(DisplayName = "It should update a Pokémon held item.")]
  public async Task Given_HeldItem_When_Update_Then_Updated()
  {
    PokemonDto created = await CreatePokemonAsync("held-item");
    Item potion = ItemBuilder.Potion(Faker, Context.World);
    await _itemRepository.SaveAsync(potion);

    UpdatePokemonPayload payload = new()
    {
      HeldItemId = new Optional<Guid?>(potion.EntityId)
    };

    PokemonDto? pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.NotNull(pokemon.HeldItem);
    Assert.Equal(potion.EntityId, pokemon.HeldItem.Id);

    payload = new()
    {
      HeldItemId = new Optional<Guid?>(null)
    };
    pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.HeldItem);
  }

  [Fact(DisplayName = "It should update a Pokémon sprite.")]
  public async Task Given_Sprite_When_Update_Then_Updated()
  {
    PokemonDto created = await CreatePokemonAsync("sprite");
    AssetDto sprite = await UploadSpriteAsync();

    UpdatePokemonPayload payload = new()
    {
      SpriteId = new Optional<Guid?>(sprite.Id)
    };

    PokemonDto? pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.NotNull(pokemon.Sprite);
    Assert.Equal(sprite.Id, pokemon.Sprite.Id);

    payload = new()
    {
      SpriteId = new Optional<Guid?>(null)
    };
    pokemon = await _pokemonService.UpdateAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.Sprite);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a Pokémon and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    PokemonDto existing = await CreatePokemonAsync("taken-key");
    PokemonDto created = await CreatePokemonAsync("other-key");

    UpdatePokemonPayload payload = new()
    {
      Key = existing.Key
    };

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Specimen.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(created.Id, exception.Data["EntityId"]);
    Assert.Equal(existing.Id, exception.Data["ConflictId"]);
    Assert.Equal(existing.Key, exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Specimen.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the held item does not exist.")]
  public async Task Given_MissingHeldItem_When_Update_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("missing-item");
    Guid missingItemId = Guid.NewGuid();

    UpdatePokemonPayload payload = new()
    {
      HeldItemId = new Optional<Guid?>(missingItemId)
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Item.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingItemId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.HeldItemId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the sprite does not exist.")]
  public async Task Given_MissingSprite_When_Update_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("missing-sprite");
    Guid missingSpriteId = Guid.NewGuid();

    UpdatePokemonPayload payload = new()
    {
      SpriteId = new Optional<Guid?>(missingSpriteId)
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Asset.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingSpriteId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.SpriteId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidAssetKindException when the sprite is not an image.")]
  public async Task Given_VideoSprite_When_Update_Then_InvalidAssetKindException()
  {
    PokemonDto created = await CreatePokemonAsync("video-sprite");
    AssetDto video = await UploadVideoAsync();

    UpdatePokemonPayload payload = new()
    {
      SpriteId = new Optional<Guid?>(video.Id)
    };

    InvalidAssetKindException exception = await Assert.ThrowsAsync<InvalidAssetKindException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(video.Id, exception.Data["AssetId"]);
    Assert.Equal(AssetKind.Image, exception.Data["ExpectedKind"]);
    Assert.Equal(AssetKind.Video, exception.Data["AttemptedKind"]);
    Assert.Equal(nameof(Specimen.SpriteId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ConstitutionOutOfRangeException when vitality exceeds HP.")]
  public async Task Given_VitalityTooHigh_When_Update_Then_ConstitutionOutOfRangeException()
  {
    PokemonDto created = await CreatePokemonAsync("vitality-too-high");
    int attemptedValue = created.Statistics.HP.Total + 1;

    UpdatePokemonPayload payload = new()
    {
      Vitality = attemptedValue
    };

    ConstitutionOutOfRangeException exception = await Assert.ThrowsAsync<ConstitutionOutOfRangeException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(created.Statistics.HP.Total, exception.Data["MaximumValue"]);
    Assert.Equal(attemptedValue, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(Specimen.Vitality), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ConstitutionOutOfRangeException when stamina exceeds HP.")]
  public async Task Given_StaminaTooHigh_When_Update_Then_ConstitutionOutOfRangeException()
  {
    PokemonDto created = await CreatePokemonAsync("stamina-too-high");
    int attemptedValue = created.Statistics.HP.Total + 1;

    UpdatePokemonPayload payload = new()
    {
      Stamina = attemptedValue
    };

    ConstitutionOutOfRangeException exception = await Assert.ThrowsAsync<ConstitutionOutOfRangeException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(created.Statistics.HP.Total, exception.Data["MaximumValue"]);
    Assert.Equal(attemptedValue, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(Specimen.Stamina), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    PokemonDto created = await CreatePokemonAsync("invalid-update");

    UpdatePokemonPayload payload = new()
    {
      Key = "not valid",
      Vitality = -1,
      Stamina = -1,
      Condition = new Optional<StatusCondition?>((StatusCondition)(-1))
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _pokemonService.UpdateAsync(created.Id, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a Pokémon.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-update");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    UpdatePokemonPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.UpdateAsync(created.Id, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should return null when changing the form of a Pokémon that does not exist.")]
  public async Task Given_NotFound_When_ChangeForm_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.ChangeFormAsync(Guid.NewGuid(), _form.EntityId));
  }

  [Fact(DisplayName = "It should change a Pokémon form.")]
  public async Task Given_ValidForm_When_ChangeForm_Then_Changed()
  {
    PokemonDto created = await CreatePokemonAsync("change-form");
    PokemonDto? damaged = await _pokemonService.UpdateAsync(created.Id, new UpdatePokemonPayload
    {
      Vitality = 1,
      Stamina = 1
    });
    Assert.NotNull(damaged);

    Form alternative = await CreateAlternativeFormAsync();

    PokemonDto? pokemon = await _pokemonService.ChangeFormAsync(created.Id, alternative.EntityId);
    Assert.NotNull(pokemon);
    Assert.Equal(damaged.Id, pokemon.Id);
    Assert.Equal(damaged.Version + 1, pokemon.Version);
    Assert.Equal(damaged.CreatedBy, pokemon.CreatedBy);
    Assert.Equal(damaged.CreatedOn, pokemon.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(alternative.EntityId, pokemon.Form.Id);
    Assert.Equal(FormCategory.Alternative, pokemon.Form.Category);
    Assert.Equal(alternative.BaseStatistics.HP, pokemon.Statistics.HP.Base);
    Assert.Equal(alternative.BaseStatistics.Attack, pokemon.Statistics.Attack.Base);
    Assert.Equal(alternative.BaseStatistics.Defense, pokemon.Statistics.Defense.Base);
    Assert.Equal(alternative.BaseStatistics.SpecialAttack, pokemon.Statistics.SpecialAttack.Base);
    Assert.Equal(alternative.BaseStatistics.SpecialDefense, pokemon.Statistics.SpecialDefense.Base);
    Assert.Equal(alternative.BaseStatistics.Speed, pokemon.Statistics.Speed.Base);

    int delta = pokemon.Statistics.HP.Total - damaged.Statistics.HP.Total;
    Assert.Equal(Math.Clamp(damaged.Vitality + delta, 0, pokemon.Statistics.HP.Total), pokemon.Vitality);
    Assert.Equal(Math.Clamp(damaged.Stamina + delta, 0, pokemon.Statistics.HP.Total), pokemon.Stamina);
  }

  [Fact(DisplayName = "It should not change a Pokémon form when it is already the target form.")]
  public async Task Given_SameForm_When_ChangeForm_Then_Unchanged()
  {
    PokemonDto created = await CreatePokemonAsync("same-form");

    PokemonDto? pokemon = await _pokemonService.ChangeFormAsync(created.Id, _form.EntityId);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Version, pokemon.Version);
    Assert.Equal(_form.EntityId, pokemon.Form.Id);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the form does not exist.")]
  public async Task Given_MissingForm_When_ChangeForm_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("missing-form");
    Guid missingFormId = Guid.NewGuid();

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.ChangeFormAsync(created.Id, missingFormId));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingFormId, exception.Data["EntityId"]);
    Assert.Equal("FormId", exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonFormException when the form belongs to another variety.")]
  public async Task Given_OtherVariety_When_ChangeForm_Then_InvalidPokemonFormException()
  {
    PokemonDto created = await CreatePokemonAsync("other-variety");

    PokemonSpecies species = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(species);
    Variety variety = VarietyBuilder.Charmander(Faker, species, Context.World);
    await _varietyRepository.SaveAsync(variety);
    Form form = FormBuilder.Charmander(Faker, variety, _ability, Context.World);
    await _formRepository.SaveAsync(form);

    InvalidPokemonFormException exception = await Assert.ThrowsAsync<InvalidPokemonFormException>(
      async () => await _pokemonService.ChangeFormAsync(created.Id, form.EntityId));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(_variety.EntityId, exception.Data["VarietyId"]);
    Assert.Equal(variety.EntityId, exception.Data["AttemptedVarietyId"]);
    Assert.Equal(form.EntityId, exception.Data["AttemptedFormId"]);
    Assert.Equal(nameof(Specimen.FormId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when changing a Pokémon form.")]
  public async Task Given_NotAllowed_When_ChangeForm_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-form");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.ChangeFormAsync(created.Id, _form.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  private async Task<PokemonDto> CreatePokemonAsync(string key)
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = key
    };
    return await _pokemonService.CreateAsync(payload);
  }

  private async Task<Form> CreateAlternativeFormAsync()
  {
    Form form = new FormBuilder(Faker)
      .WithWorld(Context.World)
      .WithVariety(_variety)
      .WithAbilities(_ability)
      .WithCategory(FormCategory.Alternative)
      .WithKey("bulbasaur-sprout")
      .WithName("Sprout Bulbasaur")
      .WithTypes(PokemonType.Grass, PokemonType.Poison)
      .WithBaseStatistics(80, 100, 123, 122, 120, 80)
      .WithYield(64, 0, 0, 0, 1, 0, 0)
      .WithSize(7, 69)
      .Build();
    await _formRepository.SaveAsync(form);
    return form;
  }

  private async Task<AssetDto> UploadSpriteAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.jpg");
    Assert.True(File.Exists(path), $"Add a JPEG file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    return asset;
  }

  private async Task<AssetDto> UploadVideoAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.mp4");
    Assert.True(File.Exists(path), $"Add an MP4 file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    Assert.Equal(AssetKind.Video, asset.Kind);
    return asset;
  }
}
