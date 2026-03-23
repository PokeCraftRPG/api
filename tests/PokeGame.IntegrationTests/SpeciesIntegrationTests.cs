using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class SpeciesIntegrationTests : IntegrationTests
{
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ISpeciesService _speciesService;

  private SpeciesAggregate _species = null!;

  public SpeciesIntegrationTests() : base()
  {
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _speciesService = ServiceProvider.GetRequiredService<ISpeciesService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _species = new SpeciesBuilder(Faker).WithWorld(World).Build();
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
      Key = "eevee",
      Name = " Eevee ",
      Category = PokemonCategory.Standard
    };

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
    Assert.Equal(2, species.Version);
    Assert.Equal(Actor, species.CreatedBy);
    Assert.Equal(DateTime.UtcNow, species.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Category, species.Category);
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
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, $" {_species.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(species);
    Assert.Equal(_species.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should replace an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = "pikachu",
      Name = " Pikachu ",
      Category = _species.Category
    };
    Guid id = _species.EntityId;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Species);

    SpeciesModel species = result.Species;
    Assert.Equal(id, species.Id);
    Assert.Equal(3, species.Version);
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Category, species.Category);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = _species.Key.Value.ToUpperInvariant(),
      Category = PokemonCategory.Standard
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

  [Fact(DisplayName = "It should update an existing species.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _species.EntityId;
    UpdateSpeciesPayload payload = new()
    {
      Name = new Optional<string>(" Pikachu ")
    };

    SpeciesModel? species = await _speciesService.UpdateAsync(id, payload);
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(2, species.Version);
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_species.Key.Value, species.Key);
    Assert.Equal(payload.Name.Value?.Trim(), species.Name);
    Assert.Equal(_species.Category, species.Category);
  }
}
