using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Forms;

[Trait(Traits.Category, Categories.Integration)]
public class FormIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IAssetService _assetService;
  private readonly IFormRepository _formRepository;
  private readonly IFormService _formService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _ability = null!;
  private Form _form = null!;
  private FormDto _seeded = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;

  public FormIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _assetService = ServiceProvider.GetRequiredService<IAssetService>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _formService = ServiceProvider.GetRequiredService<IFormService>();
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

    _seeded = (await _formService.ReadAsync(_form.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new form.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    AssetDto sprite = await UploadImageAsync();
    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(charmanderVariety.EntityId, blaze.EntityId);
    payload.Sprites = new FormSpritesPayload { DefaultId = sprite.Id };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    FormDto form = result.Form;
    Assert.NotNull(form);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, form.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, form.Id);
    }
    Assert.Equal(3, form.Version);
    Assert.Equal(Actor, form.CreatedBy);
    Assert.Equal(DateTime.UtcNow, form.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(form.CreatedBy, form.UpdatedBy);
    Assert.True(form.CreatedOn < form.UpdatedOn);

    AssertCharmander(payload, form);
    Assert.NotNull(form.Sprites);
    Assert.Equal(sprite.Id, form.Sprites.Default.Id);
    Assert.Null(form.Sprites.Shiny);
    Assert.Null(form.Sprites.Female);
    Assert.Null(form.Sprites.FemaleShiny);
  }

  [Fact(DisplayName = "It should read a form by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    FormDto? form = await _formService.ReadAsync(_form.EntityId);
    Assert.NotNull(form);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should read a form by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    FormDto? form = await _formService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(form);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should replace an existing form.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceFormPayload payload = CreateUpdatedBulbasaurPayload(_variety.EntityId, _ability.EntityId);
    payload.Key = _seeded.Key;
    Guid id = _form.EntityId;

    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    FormDto form = result.Form;
    Assert.NotNull(form);

    Assert.Equal(id, form.Id);
    Assert.Equal(4, form.Version);
    Assert.Equal(_seeded.CreatedBy, form.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, form.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, form.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, form.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedBulbasaur(payload, form);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchFormsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no form was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _formService.ReadAsync(_form.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many forms were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    TooManyResultsException<FormDto> exception = await Assert.ThrowsAsync<TooManyResultsException<FormDto>>(
      async () => await _formService.ReadAsync(_form.EntityId, charmander.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the form was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _formService.UpdateAsync(Guid.Empty, new UpdateFormPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    Ability torrent = AbilityBuilder.Torrent(Faker, Context.World);
    Ability @static = AbilityBuilder.Static(Faker, Context.World);
    await _abilityRepository.SaveAsync([blaze, torrent, @static]);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    PokemonSpecies squirtleSpecies = SpeciesBuilder.Squirtle(Faker, Context.World);
    PokemonSpecies pikachuSpecies = SpeciesBuilder.Pikachu(Faker, Context.World);
    await _speciesRepository.SaveAsync([charmanderSpecies, squirtleSpecies, pikachuSpecies]);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    Variety squirtleVariety = VarietyBuilder.Squirtle(Faker, squirtleSpecies, Context.World);
    Variety pikachuVariety = VarietyBuilder.Pikachu(Faker, pikachuSpecies, Context.World);
    await _varietyRepository.SaveAsync([charmanderVariety, squirtleVariety, pikachuVariety]);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    Form squirtle = FormBuilder.Squirtle(Faker, squirtleVariety, torrent, Context.World);
    Form pikachu = FormBuilder.Pikachu(Faker, pikachuVariety, @static, Context.World);
    await _formRepository.SaveAsync([charmander, squirtle, pikachu]);

    SearchFormsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("char");
    payload.Search.Terms.Add("squir");
    payload.Ids.AddRange([charmander.EntityId, squirtle.EntityId]);
    payload.Sort.Add(new SortOption<FormSort>(FormSort.Name, SortDirection.Descending));

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    FormDto form = Assert.Single(results.Items);
    Assert.Equal(charmander.EntityId, form.Id);
  }

  [Theory(DisplayName = "It should filter search results by variety.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_VarietyFilter_When_Search_Then_Results(bool byId)
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    SearchFormsPayload payload = new()
    {
      Variety = byId ? _variety.EntityId.ToString() : _variety.Key.Value,
      Limit = 10
    };

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormDto form = Assert.Single(results.Items);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should filter search results by category.")]
  public async Task Given_CategoryFilter_When_Search_Then_Results()
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

    SearchFormsPayload payload = new()
    {
      Category = FormCategory.Mega,
      Limit = 10
    };

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormDto form = Assert.Single(results.Items);
    Assert.Equal(mega.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should filter search results by type.")]
  public async Task Given_TypeFilter_When_Search_Then_Results()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    SearchFormsPayload payload = new()
    {
      Type = PokemonType.Poison,
      Limit = 10
    };

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormDto form = Assert.Single(results.Items);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Theory(DisplayName = "It should filter search results by ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_AbilityFilter_When_Search_Then_Results(bool byId)
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    SearchFormsPayload payload = new()
    {
      Ability = byId ? _ability.EntityId.ToString() : _ability.Key.Value,
      Limit = 10
    };

    SearchResults<FormDto> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormDto form = Assert.Single(results.Items);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a form and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(_variety.EntityId, _ability.EntityId);
    payload.Key = _seeded.Key;
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _formService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_form.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Form.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a form and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(charmanderVariety.EntityId, blaze.EntityId);
    payload.Key = _seeded.Key;
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _formService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_form.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Form.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a form and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Ability blaze = AbilityBuilder.Blaze(Faker, Context.World);
    await _abilityRepository.SaveAsync(blaze);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    Form charmander = FormBuilder.Charmander(Faker, charmanderVariety, blaze, Context.World);
    await _formRepository.SaveAsync(charmander);

    UpdateFormPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _formService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_form.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Form.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing a form with a different variety.")]
  public async Task Given_DifferentVariety_When_Replace_Then_ImmutablePropertyException()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmanderVariety = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmanderVariety);

    CreateOrReplaceFormPayload payload = CreateUpdatedBulbasaurPayload(charmanderVariety.EntityId, _ability.EntityId);

    ImmutablePropertyException<Guid> exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(
      async () => await _formService.CreateOrReplaceAsync(payload, _form.EntityId));
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(_form.EntityId, exception.Data["EntityId"]);
    Assert.Equal(_variety.EntityId, exception.Data["ExpectedValue"]);
    Assert.Equal(payload.VarietyId, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(payload.VarietyId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing a form with a different category.")]
  public async Task Given_DifferentCategory_When_Replace_Then_ImmutablePropertyException()
  {
    CreateOrReplaceFormPayload payload = CreateUpdatedBulbasaurPayload(_variety.EntityId, _ability.EntityId);
    payload.Category = FormCategory.Mega;

    ImmutablePropertyException<FormCategory> exception = await Assert.ThrowsAsync<ImmutablePropertyException<FormCategory>>(
      async () => await _formService.CreateOrReplaceAsync(payload, _form.EntityId));
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(_form.EntityId, exception.Data["EntityId"]);
    Assert.Equal(_form.Category, exception.Data["ExpectedValue"]);
    Assert.Equal(payload.Category, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(payload.Category), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the variety does not exist.")]
  public async Task Given_MissingVariety_When_Create_Then_EntityNotFoundException()
  {
    Guid missingVarietyId = Guid.NewGuid();
    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(missingVarietyId, _ability.EntityId);

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _formService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Variety.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingVarietyId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.VarietyId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the ability does not exist.")]
  public async Task Given_MissingAbility_When_Create_Then_EntityNotFoundException()
  {
    Guid missingAbilityId = Guid.NewGuid();
    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(_variety.EntityId, missingAbilityId);

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _formService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Ability.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingAbilityId, exception.Data["EntityId"]);
    Assert.Equal($"{nameof(payload.Abilities)}.{nameof(payload.Abilities.PrimaryId)}", exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceFormPayload payload = new()
    {
      VarietyId = _variety.EntityId,
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _formService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateFormPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _formService.UpdateAsync(_form.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw InvalidAssetKindException when a form sprite is not an image.")]
  public async Task Given_VideoSprite_When_Create_Then_InvalidAssetKindException()
  {
    AssetDto video = await UploadVideoAsync();
    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(_variety.EntityId, _ability.EntityId);
    payload.Key = "video-sprite-form";
    payload.Sprites = new FormSpritesPayload
    {
      DefaultId = video.Id
    };

    InvalidAssetKindException exception = await Assert.ThrowsAsync<InvalidAssetKindException>(
      async () => await _formService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(video.Id, exception.Data["AssetId"]);
    Assert.Equal(AssetKind.Image, exception.Data["ExpectedKind"]);
    Assert.Equal(AssetKind.Video, exception.Data["AttemptedKind"]);
    Assert.Equal(nameof(FormSpriteAssets.Default), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a form.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceFormPayload payload = CreateCharmanderPayload(_variety.EntityId, _ability.EntityId);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _formService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("CreateForm", exception.Data["Action"]);
    Assert.Null(exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a form.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceFormPayload payload = CreateUpdatedBulbasaurPayload(_variety.EntityId, _ability.EntityId);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _formService.CreateOrReplaceAsync(payload, _form.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(_form.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a form.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    UpdateFormPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _formService.UpdateAsync(_form.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(_form.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should update an existing form.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _form.EntityId;
    CreateOrReplaceFormPayload create = CreateUpdatedBulbasaurPayload(_variety.EntityId, _ability.EntityId);
    UpdateFormPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      Types = create.Types,
      BaseStatistics = create.BaseStatistics,
      Yield = create.Yield,
      Size = create.Size
    };

    FormDto? form = await _formService.UpdateAsync(id, payload);
    Assert.NotNull(form);

    Assert.Equal(id, form.Id);
    Assert.Equal(4, form.Version);
    Assert.Equal(_seeded.CreatedBy, form.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, form.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, form.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, form.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), form.Name);
    Assert.Equal(create.Summary?.Trim(), form.Summary);
    Assert.Equal(create.Content?.Trim(), form.Content);
    Assert.Equal(create.Types.Primary, form.Types.Primary);
    Assert.Equal(create.Types.Secondary, form.Types.Secondary);
    Assert.Equal(create.BaseStatistics.HP, form.BaseStatistics.HP);
    Assert.Equal(create.BaseStatistics.Attack, form.BaseStatistics.Attack);
    Assert.Equal(create.BaseStatistics.Defense, form.BaseStatistics.Defense);
    Assert.Equal(create.BaseStatistics.SpecialAttack, form.BaseStatistics.SpecialAttack);
    Assert.Equal(create.BaseStatistics.SpecialDefense, form.BaseStatistics.SpecialDefense);
    Assert.Equal(create.BaseStatistics.Speed, form.BaseStatistics.Speed);
    Assert.Equal(create.Yield.Experience, form.Yield.Experience);
    Assert.NotNull(form.Size);
    Assert.Equal(create.Size!.Height, form.Size.Height);
    Assert.Equal(create.Size.Weight, form.Size.Weight);
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

  private async Task<AssetDto> UploadImageAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.jpg");
    Assert.True(File.Exists(path), $"Add a JPEG file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    Assert.Equal(AssetKind.Image, asset.Kind);
    return asset;
  }

  private static CreateOrReplaceFormPayload CreateCharmanderPayload(Guid varietyId, Guid primaryAbilityId) => new()
  {
    VarietyId = varietyId,
    Category = FormCategory.Default,
    Key = "charmander",
    Name = " Charmander ",
    Summary = "  The default Charmander form.  ",
    Content = "   A Lizard Pokémon that prefers hot things.   ",
    Types = new FormTypesDto
    {
      Primary = PokemonType.Fire
    },
    Abilities = new FormAbilitiesPayload
    {
      PrimaryId = primaryAbilityId
    },
    BaseStatistics = new BaseStatisticsDto
    {
      HP = 39,
      Attack = 52,
      Defense = 43,
      SpecialAttack = 60,
      SpecialDefense = 50,
      Speed = 65
    },
    Yield = new FormYieldDto
    {
      Experience = 62,
      Attack = 1
    },
    Size = new FormSizeDto
    {
      Height = 6,
      Weight = 85
    }
  };

  private static CreateOrReplaceFormPayload CreateUpdatedBulbasaurPayload(Guid varietyId, Guid primaryAbilityId) => new()
  {
    VarietyId = varietyId,
    Category = FormCategory.Default,
    Key = "bulbasaur",
    Name = " Bulbasaur ",
    Summary = "  An updated Bulbasaur form.  ",
    Content = "   A Seed Pokémon with a plant bulb on its back from birth.   ",
    Types = new FormTypesDto
    {
      Primary = PokemonType.Grass,
      Secondary = PokemonType.Poison
    },
    Abilities = new FormAbilitiesPayload
    {
      PrimaryId = primaryAbilityId
    },
    BaseStatistics = new BaseStatisticsDto
    {
      HP = 50,
      Attack = 50,
      Defense = 50,
      SpecialAttack = 70,
      SpecialDefense = 70,
      Speed = 50
    },
    Yield = new FormYieldDto
    {
      Experience = 65,
      SpecialAttack = 1
    },
    Size = new FormSizeDto
    {
      Height = 8,
      Weight = 80
    }
  };

  private static void AssertCharmander(CreateOrReplaceFormPayload payload, FormDto form)
  {
    AssertForm(payload, form);
  }

  private static void AssertUpdatedBulbasaur(CreateOrReplaceFormPayload payload, FormDto form)
  {
    AssertForm(payload, form);
  }

  private static void AssertForm(CreateOrReplaceFormPayload payload, FormDto form)
  {
    Assert.Equal(payload.VarietyId, form.Variety.Id);
    Assert.Equal(payload.Category, form.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), form.Key);
    Assert.Equal(payload.Name?.Trim(), form.Name);
    Assert.Equal(payload.Summary?.Trim(), form.Summary);
    Assert.Equal(payload.Content?.Trim(), form.Content);
    Assert.Equal(payload.Types.Primary, form.Types.Primary);
    Assert.Equal(payload.Types.Secondary, form.Types.Secondary);
    Assert.Equal(payload.Abilities.PrimaryId, form.Abilities.Primary.Id);
    Assert.Null(form.Abilities.Secondary);
    Assert.Null(form.Abilities.Hidden);
    Assert.Equal(payload.BaseStatistics.HP, form.BaseStatistics.HP);
    Assert.Equal(payload.BaseStatistics.Attack, form.BaseStatistics.Attack);
    Assert.Equal(payload.BaseStatistics.Defense, form.BaseStatistics.Defense);
    Assert.Equal(payload.BaseStatistics.SpecialAttack, form.BaseStatistics.SpecialAttack);
    Assert.Equal(payload.BaseStatistics.SpecialDefense, form.BaseStatistics.SpecialDefense);
    Assert.Equal(payload.BaseStatistics.Speed, form.BaseStatistics.Speed);
    Assert.Equal(payload.Yield.Experience, form.Yield.Experience);
    Assert.Equal(payload.Yield.HP, form.Yield.HP);
    Assert.Equal(payload.Yield.Attack, form.Yield.Attack);
    Assert.Equal(payload.Yield.Defense, form.Yield.Defense);
    Assert.Equal(payload.Yield.SpecialAttack, form.Yield.SpecialAttack);
    Assert.Equal(payload.Yield.SpecialDefense, form.Yield.SpecialDefense);
    Assert.Equal(payload.Yield.Speed, form.Yield.Speed);
    Assert.NotNull(form.Size);
    Assert.Equal(payload.Size!.Height, form.Size.Height);
    Assert.Equal(payload.Size.Weight, form.Size.Weight);
    if (payload.Sprites is null)
    {
      Assert.Null(form.Sprites);
    }
  }
}
