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

  [Fact(DisplayName = "It should read an species by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _species.EntityId;
    SpeciesModel? species = await _speciesService.ReadAsync(id);
    Assert.NotNull(species);
    Assert.Equal(id, species.Id);
  }

  [Fact(DisplayName = "It should read an species by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, number: null, $" {_species.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read an species by number.")]
  public async Task Given_Number_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, _species.Number.Value);
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
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
