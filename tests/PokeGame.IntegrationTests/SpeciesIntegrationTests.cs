using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class SpeciesIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ISpeciesService _speciesService;

  private Region _kanto = null!;
  private Region _johto = null!;
  private Region _hoenn = null!;
  private Region _sinnoh = null!;
  private SpeciesAggregate _species = null!;

  public SpeciesIntegrationTests() : base()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _speciesService = ServiceProvider.GetRequiredService<ISpeciesService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _kanto = RegionBuilder.Kanto(Faker, World);
    _johto = RegionBuilder.Johto(Faker, World);
    _hoenn = RegionBuilder.Hoenn(Faker, World);
    _sinnoh = RegionBuilder.Sinnoh(Faker, World);
    await _regionRepository.SaveAsync([_kanto, _johto, _hoenn, _sinnoh]);

    _species = new SpeciesBuilder(Faker).WithWorld(World).WithNumber(new Number(172)).WithCategory(PokemonCategory.Baby).WithKey(new Slug("pichu")).Build();
    await _speciesRepository.SaveAsync(_species);
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 25,
      Category = PokemonCategory.Standard,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload(" kAntO   ", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload($"   {_johto.EntityId.ToString().ToUpperInvariant()} ", 22));

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Species);

    SpeciesModel species = result.Species;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, species.Id);
    }
    else
    {
      Assert.NotEqual(default, species.Id);
    }
    Assert.Equal(4, species.Version);
    Assert.Equal(Actor, species.CreatedBy);
    Assert.Equal(DateTime.UtcNow, species.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups, species.EggGroups);
    Assert.Equal(payload.Url, species.Url);
    Assert.Equal(payload.Notes.Trim(), species.Notes);

    Assert.Equal(2, species.RegionalNumbers.Count);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _kanto.EntityId && x.Number == 25);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _johto.EntityId && x.Number == 22);
  }

  [Fact(DisplayName = "It should read a species by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _species.EntityId;
    SpeciesModel? species = await _speciesService.ReadAsync(id);
    Assert.NotNull(species);
    Assert.Equal(id, species.Id);
  }

  [Fact(DisplayName = "It should read a species by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, number: null, $" {_species.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read a species by number.")]
  public async Task Given_Number_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, _species.Number.Value);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate eevee = SpeciesBuilder.Eevee(Faker, World);
    SpeciesAggregate pikachu = SpeciesBuilder.Pikachu(Faker, World);
    SpeciesAggregate raichu = SpeciesBuilder.Raichu(Faker, World);
    await _speciesRepository.SaveAsync([eevee, pikachu, raichu]);

    SearchSpeciesPayload payload = new()
    {
      Ids = [eevee.EntityId, pikachu.EntityId, raichu.EntityId, Guid.Empty],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("%chu"));
    payload.Sort.Add(new SpeciesSortOption(SpeciesSort.Key, isDescending: true));

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    SpeciesModel species = Assert.Single(results.Items);
    Assert.Equal(pikachu.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Category).")]
  public async Task Given_Category_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate eevee = SpeciesBuilder.Eevee(Faker, World);
    SpeciesAggregate riolu = SpeciesBuilder.Riolu(Faker, World);
    await _speciesRepository.SaveAsync([eevee, riolu]);

    SearchSpeciesPayload payload = new()
    {
      Ids = [eevee.EntityId, riolu.EntityId],
      Category = Faker.PickRandom(PokemonCategory.Standard, PokemonCategory.Baby)
    };

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    SpeciesModel species = Assert.Single(results.Items);
    Assert.Equal((payload.Category == PokemonCategory.Standard ? eevee : riolu).EntityId, species.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (EggGroup).")]
  public async Task Given_EggGroup_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate eevee = SpeciesBuilder.Eevee(Faker, World);
    SpeciesAggregate pikachu = SpeciesBuilder.Pikachu(Faker, World);
    SpeciesAggregate riolu = SpeciesBuilder.Riolu(Faker, World);
    await _speciesRepository.SaveAsync([eevee, pikachu, riolu]);

    SearchSpeciesPayload payload = new()
    {
      Ids = [eevee.EntityId, pikachu.EntityId, riolu.EntityId],
      EggGroup = Faker.PickRandom(EggGroup.NoEggsDiscovered, EggGroup.Fairy, EggGroup.Field)
    };

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);

    switch (payload.EggGroup)
    {
      case EggGroup.Fairy:
        Assert.Equal(1, results.Total);
        Assert.Equal(pikachu.EntityId, Assert.Single(results.Items).Id);
        break;
      case EggGroup.Field:
        Assert.Equal(2, results.Total);
        Assert.Contains(results.Items, species => species.Id == eevee.EntityId);
        Assert.Contains(results.Items, species => species.Id == pikachu.EntityId);
        break;
      default:
        Assert.Equal(1, results.Total);
        Assert.Equal(riolu.EntityId, Assert.Single(results.Items).Id);
        break;
    }
  }

  [Fact(DisplayName = "It should return the correct search results (GrowthRate).")]
  public async Task Given_GrowthRate_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate drifloon = SpeciesBuilder.Drifloon(Faker, World);
    SpeciesAggregate eevee = SpeciesBuilder.Eevee(Faker, World);
    SpeciesAggregate riolu = SpeciesBuilder.Riolu(Faker, World);
    await _speciesRepository.SaveAsync([drifloon, eevee, riolu]);

    SearchSpeciesPayload payload = new()
    {
      GrowthRate = Faker.PickRandom(GrowthRate.Fluctuating, GrowthRate.MediumFast, GrowthRate.MediumSlow)
    };

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    switch (payload.GrowthRate)
    {
      case GrowthRate.MediumFast:
        Assert.Equal(eevee.EntityId, Assert.Single(results.Items).Id);
        break;
      case GrowthRate.MediumSlow:
        Assert.Equal(riolu.EntityId, Assert.Single(results.Items).Id);
        break;
      default:
        Assert.Equal(drifloon.EntityId, Assert.Single(results.Items).Id);
        break;
    }
  }

  [Fact(DisplayName = "It should return the correct search results (RegionId).")]
  public async Task Given_RegionId_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate pikachu = SpeciesBuilder.Pikachu(Faker, World);
    pikachu.SetRegionalNumber(_kanto, new Number(25), World.OwnerId);
    await _speciesRepository.SaveAsync(pikachu);

    SearchSpeciesPayload payload = new()
    {
      RegionId = _kanto.EntityId
    };

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    SpeciesModel species = Assert.Single(results.Items);
    Assert.Equal(pikachu.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should replace an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    _species.SetRegionalNumber(_kanto, new Number(1), World.OwnerId);
    _species.SetRegionalNumber(_hoenn, new Number(25), World.OwnerId);
    await _speciesRepository.SaveAsync(_species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = _species.Number.Value,
      Category = _species.Category,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload(" kAntO   ", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload($"   {_johto.EntityId.ToString().ToUpperInvariant()} ", 22));
    Guid id = _species.EntityId;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Species);

    SpeciesModel species = result.Species;
    Assert.Equal(id, species.Id);
    Assert.Equal(8, species.Version);
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups, species.EggGroups);
    Assert.Equal(payload.Url, species.Url);
    Assert.Equal(payload.Notes.Trim(), species.Notes);

    Assert.Equal(2, species.RegionalNumbers.Count);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _kanto.EntityId && x.Number == 25);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _johto.EntityId && x.Number == 22);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 172,
      Category = PokemonCategory.Baby,
      Key = _species.Key.Value.ToUpperInvariant(),
      Name = "Pichu",
      CatchRate = 190,
      EggCycles = 10
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Species", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(_species.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a number conflict.")]
  public async Task Given_NumberConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = _species.Number.Value,
      Category = PokemonCategory.Baby,
      Key = "pichu-z",
      CatchRate = 190,
      EggCycles = 10
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<int>>(async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Species", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(_species.Number.Value, exception.AttemptedValue);
    Assert.Equal("Number", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw RegionalNumberConflictException when there is a regional number conflict.")]
  public async Task Given_RegionalNumberConflict_When_Create_Then_RegionalNumberConflictException()
  {
    Number number = new(21);
    _species.SetRegionalNumber(_johto, number, World.OwnerId);
    await _speciesRepository.SaveAsync(_species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 25,
      Key = "pikachu",
      CatchRate = 190,
      EggCycles = 10
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload(_johto.Key.Value, number.Value));
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<RegionalNumberConflictException>(async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal(id, exception.SpeciesId);
    Assert.Equal(_species.EntityId, exception.ConflictId);
    Assert.Equal(_johto.EntityId, exception.RegionId);
    Assert.Equal(number.Value, exception.Number);
    Assert.Equal("RegionalNumbers", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing species.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    _species.SetRegionalNumber(_kanto, new Number(1), World.OwnerId);
    _species.SetRegionalNumber(_hoenn, new Number(163), World.OwnerId);
    _species.SetRegionalNumber(_sinnoh, new Number(25), World.OwnerId);
    await _speciesRepository.SaveAsync(_species);

    Guid id = _species.EntityId;
    UpdateSpeciesPayload payload = new()
    {
      Name = new Optional<string>("Pikachu"),
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact.")
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload(" kAntO   ", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload($"   {_johto.EntityId.ToString().ToUpperInvariant()} ", 22));
    payload.RegionalNumbers.Add(new RegionalNumberPayload(_sinnoh.EntityId.ToString(), 0));

    SpeciesModel? species = await _speciesService.UpdateAsync(id, payload);
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(8, species.Version);
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_species.Key.Value, species.Key);
    Assert.Equal(payload.Name.Value?.Trim(), species.Name);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups, species.EggGroups);
    Assert.Equal(payload.Url.Value, species.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), species.Notes);

    Assert.Equal(3, species.RegionalNumbers.Count);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _kanto.EntityId && x.Number == 25);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _johto.EntityId && x.Number == 22);
    Assert.Contains(species.RegionalNumbers, x => x.Region.Id == _hoenn.EntityId && x.Number == 163);
  }
}
