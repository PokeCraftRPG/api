using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Species;

[Trait(Traits.Category, Categories.Integration)]
public class SpeciesIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ISpeciesService _speciesService;

  private Region _region = null!;
  private PokemonSpecies _species = null!;
  private SpeciesDto _seeded = null!;

  public SpeciesIntegrationTests()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _speciesService = ServiceProvider.GetRequiredService<ISpeciesService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _region = RegionBuilder.Kanto(Faker, Context.World);
    await _regionRepository.SaveAsync(_region);

    _species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(_species);

    _seeded = (await _speciesService.ReadAsync(_species.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    SpeciesDto species = result.Species;
    Assert.NotNull(species);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, species.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, species.Id);
    }
    Assert.Equal(2, species.Version);
    Assert.Equal(Actor, species.CreatedBy);
    Assert.Equal(DateTime.UtcNow, species.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(species.CreatedBy, species.UpdatedBy);
    Assert.True(species.CreatedOn < species.UpdatedOn);

    AssertCharmander(payload, species);
  }

  [Fact(DisplayName = "It should create a species with a regional number.")]
  public async Task Given_RegionalNumber_When_Create_Then_Created()
  {
    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    payload.RegionalNumbers.Add(new RegionalNumberPayload
    {
      RegionId = _region.EntityId,
      Number = 4
    });

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload);
    Assert.True(result.Created);
    SpeciesDto species = result.Species;

    Assert.Equal(3, species.Version);
    RegionalNumberDto regionalNumber = Assert.Single(species.RegionalNumbers);
    Assert.Equal(_region.EntityId, regionalNumber.Region.Id);
    Assert.Equal(4, regionalNumber.Number);
  }

  [Fact(DisplayName = "It should read a species by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    SpeciesDto? species = await _speciesService.ReadAsync(_species.EntityId);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read a species by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    SpeciesDto? species = await _speciesService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read a species by number.")]
  public async Task Given_Number_When_Read_Then_Read()
  {
    SpeciesDto? species = await _speciesService.ReadAsync(number: _seeded.Number);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read a species by regional number.")]
  public async Task Given_RegionalNumber_When_Read_Then_Read()
  {
    await _speciesService.SetRegionalNumberAsync(_species.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 });

    SpeciesDto? species = await _speciesService.ReadAsync(_region.Key.Value, 1);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should replace an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceSpeciesPayload payload = CreateUpdatedBulbasaurPayload();
    payload.Key = _seeded.Key;
    Guid id = _species.EntityId;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    SpeciesDto species = result.Species;
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(3, species.Version);
    Assert.Equal(_seeded.CreatedBy, species.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedBulbasaur(payload, species);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchSpeciesPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<SpeciesDto> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no species was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _speciesService.ReadAsync(_species.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many species were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmander);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _speciesService.ReadAsync(_species.EntityId, number: null, charmander.Key.Value));
    TooManyResultsException<SpeciesDto> tooMany = Assert.IsType<TooManyResultsException<SpeciesDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the species was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _speciesService.UpdateAsync(Guid.Empty, new UpdateSpeciesPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    PokemonSpecies squirtle = SpeciesBuilder.Squirtle(Faker, Context.World);
    PokemonSpecies pikachu = SpeciesBuilder.Pikachu(Faker, Context.World);
    await _speciesRepository.SaveAsync([charmander, squirtle, pikachu]);

    SearchSpeciesPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("char");
    payload.Search.Terms.Add("squir");
    payload.Ids.AddRange([charmander.EntityId, squirtle.EntityId]);
    payload.Sort.Add(new SortOption<SpeciesSort>(SpeciesSort.Name, SortDirection.Descending));

    SearchResults<SpeciesDto> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    SpeciesDto species = Assert.Single(results.Items);
    Assert.Equal(charmander.EntityId, species.Id);
  }

  [Theory(DisplayName = "It should filter search results by region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_RegionFilter_When_Search_Then_Results(bool byId)
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync(johto);

    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    PokemonSpecies squirtle = SpeciesBuilder.Squirtle(Faker, Context.World);
    await _speciesRepository.SaveAsync([charmander, squirtle]);

    await _speciesService.SetRegionalNumberAsync(_species.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 });
    await _speciesService.SetRegionalNumberAsync(charmander.EntityId, johto.EntityId, new SetRegionalNumberPayload { Number = 4 });

    SearchSpeciesPayload payload = new()
    {
      Region = byId ? _region.EntityId.ToString() : _region.Key.Value,
      Limit = 10
    };
    payload.Sort.Add(new SortOption<SpeciesSort>(SpeciesSort.Number));

    SearchResults<SpeciesDto> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    SpeciesDto species = Assert.Single(results.Items);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should return empty search results when no species matches the region filter.")]
  public async Task Given_RegionFilter_When_Search_Then_EmptyResults()
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync(johto);

    await _speciesService.SetRegionalNumberAsync(_species.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 });

    SearchSpeciesPayload payload = new()
    {
      Region = johto.Key.Value,
      Limit = 10
    };

    SearchResults<SpeciesDto> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should set a regional number.")]
  public async Task Given_Exists_When_SetRegionalNumber_Then_Set()
  {
    SpeciesDto species = await _speciesService.SetRegionalNumberAsync(
      _species.EntityId,
      _region.EntityId,
      new SetRegionalNumberPayload { Number = 1 });

    RegionalNumberDto regionalNumber = Assert.Single(species.RegionalNumbers);
    Assert.Equal(_region.EntityId, regionalNumber.Region.Id);
    Assert.Equal(1, regionalNumber.Number);
    Assert.Equal(Actor, regionalNumber.CreatedBy);
    Assert.Equal(DateTime.UtcNow, regionalNumber.CreatedOn, TimeSpan.FromSeconds(10));
  }

  [Fact(DisplayName = "It should remove a regional number.")]
  public async Task Given_Exists_When_RemoveRegionalNumber_Then_Removed()
  {
    await _speciesService.SetRegionalNumberAsync(_species.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 });

    SpeciesDto? species = await _speciesService.RemoveRegionalNumberAsync(_species.EntityId, _region.EntityId);
    Assert.NotNull(species);
    Assert.Empty(species.RegionalNumbers);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a species and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    payload.Key = _seeded.Key;
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(PokemonSpecies.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw NumberAlreadyUsedException when creating a species and the number conflicts.")]
  public async Task Given_NumberConflict_When_Create_Then_NumberAlreadyUsedException()
  {
    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    payload.Number = _seeded.Number;
    Guid id = Guid.NewGuid();

    NumberAlreadyUsedException exception = await Assert.ThrowsAsync<NumberAlreadyUsedException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(id, exception.SpeciesId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Null(exception.RegionId);
    Assert.Equal(payload.Number, exception.AttemptedNumber);
    Assert.Equal(nameof(PokemonSpecies.Number), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw NumberAlreadyUsedException when the regional number conflicts.")]
  public async Task Given_RegionalNumberConflict_When_Set_Then_NumberAlreadyUsedException()
  {
    await _speciesService.SetRegionalNumberAsync(_species.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 });

    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmander);

    NumberAlreadyUsedException exception = await Assert.ThrowsAsync<NumberAlreadyUsedException>(
      async () => await _speciesService.SetRegionalNumberAsync(charmander.EntityId, _region.EntityId, new SetRegionalNumberPayload { Number = 1 }));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(charmander.EntityId, exception.SpeciesId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(_region.EntityId, exception.RegionId);
    Assert.Equal(charmander.Number.Value, exception.AttemptedNumber);
    Assert.Equal(nameof(PokemonSpecies.Number), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a species and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmander);

    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    payload.Key = _seeded.Key;
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(PokemonSpecies.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a species and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    PokemonSpecies charmander = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmander);

    UpdateSpeciesPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = charmander.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _speciesService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(PokemonSpecies.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing a species with a different number.")]
  public async Task Given_DifferentNumber_When_Replace_Then_ImmutablePropertyException()
  {
    CreateOrReplaceSpeciesPayload payload = CreateUpdatedBulbasaurPayload();
    payload.Number = 999;

    ImmutablePropertyException<int> exception = await Assert.ThrowsAsync<ImmutablePropertyException<int>>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, _species.EntityId));
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(_species.EntityId, exception.EntityId);
    Assert.Equal(_seeded.Number, exception.ExpectedValue);
    Assert.Equal(payload.Number, exception.AttemptedValue);
    Assert.Equal(nameof(payload.Number), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing a species with a different category.")]
  public async Task Given_DifferentCategory_When_Replace_Then_ImmutablePropertyException()
  {
    CreateOrReplaceSpeciesPayload payload = CreateUpdatedBulbasaurPayload();
    payload.Category = SpeciesCategory.Legendary;

    ImmutablePropertyException<SpeciesCategory> exception = await Assert.ThrowsAsync<ImmutablePropertyException<SpeciesCategory>>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, _species.EntityId));
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(_species.EntityId, exception.EntityId);
    Assert.Equal(_seeded.Category, exception.ExpectedValue);
    Assert.Equal(payload.Category, exception.AttemptedValue);
    Assert.Equal(nameof(payload.Category), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw RegionsNotFoundException when a regional region does not exist.")]
  public async Task Given_MissingRegion_When_Create_Then_RegionsNotFoundException()
  {
    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();
    Guid missingRegionId = Guid.NewGuid();
    payload.RegionalNumbers.Add(new RegionalNumberPayload
    {
      RegionId = missingRegionId,
      Number = 4
    });

    RegionsNotFoundException exception = await Assert.ThrowsAsync<RegionsNotFoundException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal([missingRegionId], exception.RegionIds);
    Assert.Equal(nameof(payload.RegionalNumbers), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 1,
      Key = string.Empty,
      Eggs = new SpeciesEggsDto { Cycles = 20, PrimaryGroup = EggGroup.Monster }
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _speciesService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateSpeciesPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _speciesService.UpdateAsync(_species.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a species.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceSpeciesPayload payload = CreateCharmanderPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateSpecies", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a species.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceSpeciesPayload payload = CreateUpdatedBulbasaurPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _speciesService.CreateOrReplaceAsync(payload, _species.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_species.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a species.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateSpeciesPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _speciesService.UpdateAsync(_species.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_species.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing species.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _species.EntityId;
    CreateOrReplaceSpeciesPayload create = CreateUpdatedBulbasaurPayload();
    UpdateSpeciesPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      BaseFriendship = create.BaseFriendship,
      CatchRate = create.CatchRate,
      GrowthRate = create.GrowthRate,
      Eggs = create.Eggs
    };

    SpeciesDto? species = await _speciesService.UpdateAsync(id, payload);
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(3, species.Version);
    Assert.Equal(_seeded.CreatedBy, species.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), species.Name);
    Assert.Equal(create.Summary?.Trim(), species.Summary);
    Assert.Equal(create.Content?.Trim(), species.Content);
    Assert.Equal(create.BaseFriendship, species.BaseFriendship);
    Assert.Equal(create.CatchRate, species.CatchRate);
    Assert.Equal(create.GrowthRate, species.GrowthRate);
    Assert.Equal(create.Eggs.Cycles, species.Eggs.Cycles);
    Assert.Equal(create.Eggs.PrimaryGroup, species.Eggs.PrimaryGroup);
    Assert.Equal(create.Eggs.SecondaryGroup, species.Eggs.SecondaryGroup);
  }

  private static CreateOrReplaceSpeciesPayload CreateCharmanderPayload() => new()
  {
    Number = 4,
    Category = SpeciesCategory.Standard,
    Key = "charmander",
    Name = " Charmander ",
    Summary = "  A lizard Pokémon.  ",
    Content = "   It has a preference for hot things.   ",
    BaseFriendship = 70,
    CatchRate = 45,
    GrowthRate = GrowthRate.MediumSlow,
    Eggs = new SpeciesEggsDto
    {
      Cycles = 20,
      PrimaryGroup = EggGroup.Monster,
      SecondaryGroup = EggGroup.Dragon
    }
  };

  private static CreateOrReplaceSpeciesPayload CreateUpdatedBulbasaurPayload() => new()
  {
    Number = 1,
    Category = SpeciesCategory.Standard,
    Key = "bulbasaur",
    Name = " Bulbasaur ",
    Summary = "  An updated seed Pokémon.  ",
    Content = "   There is a plant seed on its back right from the day this Pokémon is born.   ",
    BaseFriendship = 80,
    CatchRate = 50,
    GrowthRate = GrowthRate.MediumSlow,
    Eggs = new SpeciesEggsDto
    {
      Cycles = 25,
      PrimaryGroup = EggGroup.Monster,
      SecondaryGroup = EggGroup.Grass
    }
  };

  private static void AssertCharmander(CreateOrReplaceSpeciesPayload payload, SpeciesDto species)
  {
    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), species.Key);
    Assert.Equal(payload.Name?.Trim(), species.Name);
    Assert.Equal(payload.Summary?.Trim(), species.Summary);
    Assert.Equal(payload.Content?.Trim(), species.Content);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.Eggs.Cycles, species.Eggs.Cycles);
    Assert.Equal(payload.Eggs.PrimaryGroup, species.Eggs.PrimaryGroup);
    Assert.Equal(payload.Eggs.SecondaryGroup, species.Eggs.SecondaryGroup);
  }

  private static void AssertUpdatedBulbasaur(CreateOrReplaceSpeciesPayload payload, SpeciesDto species)
  {
    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), species.Key);
    Assert.Equal(payload.Name?.Trim(), species.Name);
    Assert.Equal(payload.Summary?.Trim(), species.Summary);
    Assert.Equal(payload.Content?.Trim(), species.Content);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.Eggs.Cycles, species.Eggs.Cycles);
    Assert.Equal(payload.Eggs.PrimaryGroup, species.Eggs.PrimaryGroup);
    Assert.Equal(payload.Eggs.SecondaryGroup, species.Eggs.SecondaryGroup);
  }
}
