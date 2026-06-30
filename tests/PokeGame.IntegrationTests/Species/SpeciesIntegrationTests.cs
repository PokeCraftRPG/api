using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Species;

[Trait(Traits.Category, Categories.Integration)]
public class SpeciesIntegrationTests : IntegrationTests
{
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ISpeciesService _speciesService;
  private readonly IWorldRepository _worldRepository;

  private PokemonSpecies _species = null!;

  public SpeciesIntegrationTests() : base()
  {
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _speciesService = ServiceProvider.GetRequiredService<ISpeciesService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _species = new SpeciesBuilder(Faker).WithWorld(Context.World).Build();
    _speciesRepository.Add(_species);
    await Context.SaveChangesAsync();
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 443,
      Category = PokemonCategory.Standard,
      Key = "Gible",
      Name = " Gible ",
      Description = "  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  ",
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    SpeciesModel species = result.Species;
    Assert.NotNull(species);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, species.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, species.Id);
    }
    Assert.Equal(1, species.Version);
    Assert.Equal(Actor, species.CreatedBy);
    Assert.Equal(DateTime.UtcNow, species.CreatedOn, TimeSpan.FromSeconds(1));
    Assert.Equal(species.CreatedBy, species.UpdatedBy);
    Assert.Equal(species.CreatedOn, species.UpdatedOn);

    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Description.Trim(), species.Description);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups, species.EggGroups);
  }

  [Fact(DisplayName = "It should read a species by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(_species.Id);
    Assert.NotNull(species);
    Assert.Equal(_species.Id, species.Id);
  }

  [Fact(DisplayName = "It should read a species by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, number: null, $" {_species.Key.ToUpperInvariant()} ");
    Assert.NotNull(species);
    Assert.Equal(_species.Id, species.Id);
  }

  [Fact(DisplayName = "It should read a species by number.")]
  public async Task Given_Number_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, _species.Number);
    Assert.NotNull(species);
    Assert.Equal(_species.Id, species.Id);
  }

  [Fact(DisplayName = "It should replace an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = _species.Number,
      Category = _species.Category,
      Key = "Gible",
      Name = " Gible ",
      Description = "  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  ",
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, _species.Id);
    Assert.False(result.Created);
    SpeciesModel species = result.Species;
    Assert.NotNull(species);

    Assert.Equal(_species.Id, species.Id);
    Assert.Equal(_species.Version, species.Version);
    Assert.Equal(_species.CreatedBy, species.CreatedBy.Id);
    Assert.Equal(_species.CreatedOn, species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Description.Trim(), species.Description);
  }

  [Fact(DisplayName = "It should return empty search results when no species is matching.")]
  public async Task Given_NoneMatching_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder().Build();

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(new SearchSpeciesPayload());
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the user does not own the world.")]
  public async Task Given_NotOwner_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();

    Assert.Null(await _speciesService.ReadAsync(_species.Id, _species.Number, $" {_species.Key.ToUpperInvariant()} "));
  }

  [Fact(DisplayName = "It should return null when the species does not exist.")]
  public async Task Given_NotExist_When_Update_Then_NullReturned()
  {
    Assert.Null(await _speciesService.UpdateAsync(Guid.Empty, new UpdateSpeciesPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_CorrectResults()
  {
    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    _worldRepository.Add(world);

    PokemonSpecies eevee = SpeciesBuilder.Eevee(Faker, world);
    PokemonSpecies pichu = SpeciesBuilder.Pichu(Faker, world);
    PokemonSpecies pikachu = SpeciesBuilder.Pikachu(Faker, world);
    PokemonSpecies raichu = SpeciesBuilder.Raichu(Faker, world);
    _speciesRepository.Add(eevee, pichu, pikachu, raichu);

    Context.World = world;
    await Context.SaveChangesAsync();

    SearchSpeciesPayload payload = new()
    {
      Skip = 1,
      Limit = 1
    };
    payload.Ids.AddRange(eevee.Id, Guid.Empty, pikachu.Id, raichu.Id);
    payload.Search.Terms.Add(new SearchTerm("%chU"));
    payload.Sort.Add(new SpeciesSortOption(SpeciesSort.Name, isDescending: true));

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(2, results.Total);
    Assert.Equal(pikachu.Id, Assert.Single(results.Items).Id);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (CreateOrReplace).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    PokemonSpecies eevee = SpeciesBuilder.Eevee(Faker, Context.World);
    _speciesRepository.Add(eevee);
    await Context.SaveChangesAsync();

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = exists ? eevee.Number : 443,
      Category = exists ? eevee.Category : PokemonCategory.Standard,
      Key = _species.Key,
      Name = " Gible ",
      Description = "  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  ",
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };
    Guid? id = exists ? eevee.Id : null;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(PokemonSpecies.ResourceKind, exception.ResourceKind);
    if (id.HasValue)
    {
      Assert.Equal(id.Value, exception.ResourceId);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, exception.ResourceId);
    }
    Assert.Equal(_species.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (Update).")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    PokemonSpecies eevee = SpeciesBuilder.Eevee(Faker, Context.World);
    _speciesRepository.Add(eevee);
    await Context.SaveChangesAsync();

    UpdateSpeciesPayload payload = new()
    {
      Key = _species.Key
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _speciesService.UpdateAsync(eevee.Id, payload));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(PokemonSpecies.ResourceKind, exception.ResourceKind);
    Assert.Equal(eevee.Id, exception.ResourceId);
    Assert.Equal(_species.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new species.")]
  public async Task Given_NotExist_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 443,
      Category = PokemonCategory.Standard,
      Key = "Gible",
      Name = " Gible ",
      Description = "  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  ",
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.CreateSpecies, exception.Action);
    Assert.Equal(world.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    PokemonSpecies species = new SpeciesBuilder(Faker).WithWorld(world).Build();
    _speciesRepository.Add(species);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 443,
      Category = PokemonCategory.Standard,
      Key = "Gible",
      Name = " Gible ",
      Description = "  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  ",
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.CreateOrReplaceAsync(payload, species.Id));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(species.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing species.")]
  public async Task Given_Exists_When_Update_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    PokemonSpecies species = new SpeciesBuilder(Faker).WithWorld(world).Build();
    _speciesRepository.Add(species);

    Context.World = world;
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.UpdateAsync(species.Id, new UpdateSpeciesPayload()));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(species.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many species were found.")]
  public async Task Given_ManySpeciesFound_When_Read_Then_TooManyResultsException()
  {
    PokemonSpecies eevee = SpeciesBuilder.Eevee(Faker, Context.World);
    PokemonSpecies pichu = SpeciesBuilder.Pichu(Faker, Context.World);
    _speciesRepository.Add(eevee, pichu);
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<TooManyResultsException<SpeciesModel>>(async () => await _speciesService.ReadAsync(_species.Id, pichu.Number, $" {eevee.Key.ToUpperInvariant()} "));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(3, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing species.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _species.Id;
    UpdateSpeciesPayload payload = new()
    {
      Key = "Gible",
      Name = new Optional<string>(" Gible "),
      Description = new Optional<string>("  Gible is a Gen IV Dragon/Ground Pokémon that evolves into Gabite, then Garchomp, and lives in warm caves with powerful jaws.  "),
      BaseFriendship = 50,
      CatchRate = 45,
      GrowthRate = GrowthRate.Slow,
      EggCycles = 40,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };

    SpeciesModel? species = await _speciesService.UpdateAsync(id, payload);
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(_species.Version, species.Version);
    Assert.Equal(_species.CreatedBy, species.CreatedBy.Id);
    Assert.Equal(_species.CreatedOn, species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), species.Key);
    Assert.Equal(payload.Name.Value?.Trim(), species.Name);
    Assert.Equal(payload.Description.Value?.Trim(), species.Description);
  }
}
