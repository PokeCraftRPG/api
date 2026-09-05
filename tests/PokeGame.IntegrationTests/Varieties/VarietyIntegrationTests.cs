using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Varieties;

[Trait(Traits.Category, Categories.Integration)]
public class VarietyIntegrationTests : IntegrationTests
{
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;
  private readonly IVarietyService _varietyService;

  private PokemonSpecies _species = null!;
  private Variety _variety = null!;
  private VarietyDto _seeded = null!;

  public VarietyIntegrationTests()
  {
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
    _varietyService = ServiceProvider.GetRequiredService<IVarietyService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Bulbasaur(Faker, _species, Context.World);
    await _varietyRepository.SaveAsync(_variety);

    _seeded = (await _varietyService.ReadAsync(_variety.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new variety.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    CreateOrReplaceVarietyPayload payload = CreateCharmanderPayload(charmanderSpecies.EntityId);
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    VarietyDto variety = result.Variety;
    Assert.NotNull(variety);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, variety.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, variety.Id);
    }
    Assert.Equal(4, variety.Version);
    Assert.Equal(Actor, variety.CreatedBy);
    Assert.Equal(DateTime.UtcNow, variety.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(variety.CreatedBy, variety.UpdatedBy);
    Assert.True(variety.CreatedOn < variety.UpdatedOn);

    AssertCharmander(payload, variety);
  }

  [Fact(DisplayName = "It should read a variety by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    VarietyDto? variety = await _varietyService.ReadAsync(_variety.EntityId);
    Assert.NotNull(variety);
    Assert.Equal(_variety.EntityId, variety.Id);
  }

  [Fact(DisplayName = "It should read a variety by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    VarietyDto? variety = await _varietyService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(variety);
    Assert.Equal(_variety.EntityId, variety.Id);
  }

  [Fact(DisplayName = "It should replace an existing variety.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceVarietyPayload payload = CreateUpdatedBulbasaurPayload(_species.EntityId);
    payload.Key = _seeded.Key;
    Guid id = _variety.EntityId;

    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    VarietyDto variety = result.Variety;
    Assert.NotNull(variety);

    Assert.Equal(id, variety.Id);
    Assert.Equal(6, variety.Version);
    Assert.Equal(_seeded.CreatedBy, variety.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, variety.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, variety.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, variety.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedBulbasaur(payload, variety);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchVarietiesPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<VarietyDto> results = await _varietyService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no variety was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _varietyService.ReadAsync(_variety.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many varieties were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmander);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _varietyService.ReadAsync(_variety.EntityId, charmander.Key.Value));
    TooManyResultsException<VarietyDto> tooMany = Assert.IsType<TooManyResultsException<VarietyDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the variety was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _varietyService.UpdateAsync(Guid.Empty, new UpdateVarietyPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    PokemonSpecies squirtleSpecies = SpeciesBuilder.Squirtle(Faker, Context.World);
    PokemonSpecies pikachuSpecies = SpeciesBuilder.Pikachu(Faker, Context.World);
    await _speciesRepository.SaveAsync([charmanderSpecies, squirtleSpecies, pikachuSpecies]);

    Variety charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    Variety squirtle = VarietyBuilder.Squirtle(Faker, squirtleSpecies, Context.World);
    Variety pikachu = VarietyBuilder.Pikachu(Faker, pikachuSpecies, Context.World);
    await _varietyRepository.SaveAsync([charmander, squirtle, pikachu]);

    SearchVarietiesPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("char");
    payload.Search.Terms.Add("squir");
    payload.Ids.AddRange([charmander.EntityId, squirtle.EntityId]);
    payload.Sort.Add(new SortOption<VarietySort>(VarietySort.Name, SortDirection.Descending));

    SearchResults<VarietyDto> results = await _varietyService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    VarietyDto variety = Assert.Single(results.Items);
    Assert.Equal(charmander.EntityId, variety.Id);
  }

  [Theory(DisplayName = "It should filter search results by species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_SpeciesFilter_When_Search_Then_Results(bool byId)
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmander);

    SearchVarietiesPayload payload = new()
    {
      Species = byId ? _species.EntityId.ToString() : _species.Key.Value,
      Limit = 10
    };

    SearchResults<VarietyDto> results = await _varietyService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    VarietyDto variety = Assert.Single(results.Items);
    Assert.Equal(_variety.EntityId, variety.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a variety and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceVarietyPayload payload = CreateCharmanderPayload(_species.EntityId);
    payload.Key = _seeded.Key;
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _varietyService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_variety.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Variety.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a variety and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmander);

    CreateOrReplaceVarietyPayload payload = CreateCharmanderPayload(charmanderSpecies.EntityId);
    payload.Key = _seeded.Key;
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _varietyService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_variety.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Variety.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a variety and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    Variety charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(charmander);

    UpdateVarietyPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _varietyService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_variety.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Variety.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing a variety with a different species.")]
  public async Task Given_DifferentSpecies_When_Replace_Then_ImmutablePropertyException()
  {
    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);

    CreateOrReplaceVarietyPayload payload = CreateUpdatedBulbasaurPayload(charmanderSpecies.EntityId);

    ImmutablePropertyException<Guid> exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(
      async () => await _varietyService.CreateOrReplaceAsync(payload, _variety.EntityId));
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(_variety.EntityId, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ExpectedValue);
    Assert.Equal(payload.SpeciesId, exception.AttemptedValue);
    Assert.Equal(nameof(payload.SpeciesId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the species does not exist.")]
  public async Task Given_MissingSpecies_When_Create_Then_EntityNotFoundException()
  {
    Guid missingSpeciesId = Guid.NewGuid();
    CreateOrReplaceVarietyPayload payload = CreateCharmanderPayload(missingSpeciesId);

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _varietyService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(missingSpeciesId, exception.EntityId);
    Assert.Equal(nameof(payload.SpeciesId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceVarietyPayload payload = new()
    {
      SpeciesId = _species.EntityId,
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _varietyService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateVarietyPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _varietyService.UpdateAsync(_variety.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a variety.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceVarietyPayload payload = CreateCharmanderPayload(_species.EntityId);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _varietyService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateVariety", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a variety.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceVarietyPayload payload = CreateUpdatedBulbasaurPayload(_species.EntityId);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _varietyService.CreateOrReplaceAsync(payload, _variety.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_variety.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a variety.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateVarietyPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _varietyService.UpdateAsync(_variety.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_variety.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing variety.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _variety.EntityId;
    CreateOrReplaceVarietyPayload create = CreateUpdatedBulbasaurPayload(_species.EntityId);
    UpdateVarietyPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      CanChangeForm = create.CanChangeForm,
      GenderRatio = new Optional<int?>(create.GenderRatio),
      Genus = new Optional<string>(create.Genus)
    };

    VarietyDto? variety = await _varietyService.UpdateAsync(id, payload);
    Assert.NotNull(variety);

    Assert.Equal(id, variety.Id);
    Assert.Equal(6, variety.Version);
    Assert.Equal(_seeded.CreatedBy, variety.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, variety.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, variety.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, variety.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), variety.Name);
    Assert.Equal(create.Summary?.Trim(), variety.Summary);
    Assert.Equal(create.Content?.Trim(), variety.Content);
    Assert.Equal(create.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(create.GenderRatio, variety.GenderRatio);
    Assert.Equal(create.Genus?.Trim(), variety.Genus);
  }

  private static CreateOrReplaceVarietyPayload CreateCharmanderPayload(Guid speciesId) => new()
  {
    SpeciesId = speciesId,
    IsDefault = true,
    Key = "charmander",
    Name = " Charmander ",
    Summary = "  The default Charmander form.  ",
    Content = "   A Lizard Pokémon that prefers hot things.   ",
    CanChangeForm = false,
    GenderRatio = 1,
    Genus = " Lizard "
  };

  private static CreateOrReplaceVarietyPayload CreateUpdatedBulbasaurPayload(Guid speciesId) => new()
  {
    SpeciesId = speciesId,
    IsDefault = true,
    Key = "bulbasaur",
    Name = " Bulbasaur ",
    Summary = "  An updated Bulbasaur form.  ",
    Content = "   A Seed Pokémon with a plant bulb on its back from birth.   ",
    CanChangeForm = true,
    GenderRatio = 2,
    Genus = " Seed "
  };

  private static void AssertCharmander(CreateOrReplaceVarietyPayload payload, VarietyDto variety)
  {
    Assert.Equal(payload.SpeciesId, variety.Species.Id);
    Assert.Equal(payload.IsDefault, variety.IsDefault);
    Assert.Equal(SlugHelper.Format(payload.Key), variety.Key);
    Assert.Equal(payload.Name?.Trim(), variety.Name);
    Assert.Equal(payload.Summary?.Trim(), variety.Summary);
    Assert.Equal(payload.Content?.Trim(), variety.Content);
    Assert.Equal(payload.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(payload.GenderRatio, variety.GenderRatio);
    Assert.Equal(payload.Genus?.Trim(), variety.Genus);
  }

  private static void AssertUpdatedBulbasaur(CreateOrReplaceVarietyPayload payload, VarietyDto variety)
  {
    Assert.Equal(payload.SpeciesId, variety.Species.Id);
    Assert.Equal(payload.IsDefault, variety.IsDefault);
    Assert.Equal(SlugHelper.Format(payload.Key), variety.Key);
    Assert.Equal(payload.Name?.Trim(), variety.Name);
    Assert.Equal(payload.Summary?.Trim(), variety.Summary);
    Assert.Equal(payload.Content?.Trim(), variety.Content);
    Assert.Equal(payload.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(payload.GenderRatio, variety.GenderRatio);
    Assert.Equal(payload.Genus?.Trim(), variety.Genus);
  }
}
