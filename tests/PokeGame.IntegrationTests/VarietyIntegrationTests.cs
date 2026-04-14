using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class VarietyIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;
  private readonly IVarietyService _varietyService;

  private Move _agility = null!;
  private Move _quickAttack = null!;
  private Move _thunderPunch = null!;
  private Move _thunderShock = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;

  public VarietyIntegrationTests() : base()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
    _varietyService = ServiceProvider.GetRequiredService<IVarietyService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _agility = MoveBuilder.Agility(Faker, World);
    _quickAttack = MoveBuilder.QuickAttack(Faker, World);
    _thunderPunch = MoveBuilder.ThunderPunch(Faker, World);
    _thunderShock = MoveBuilder.ThunderShock(Faker, World);
    await _moveRepository.SaveAsync([_agility, _quickAttack, _thunderPunch, _thunderShock]);

    _species = SpeciesBuilder.Raichu(Faker, World);
    await _speciesRepository.SaveAsync(_species);

    _variety = new VarietyBuilder(Faker).WithWorld(World).WithSpecies(_species).Build();
    await _varietyRepository.SaveAsync(_variety);
  }

  [Theory(DisplayName = "It should create a new variety.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = $" {(withId ? _species.EntityId.ToString() : _species.Key.Value).ToUpperInvariant()} ",
      IsDefault = true,
      Key = "raichu",
      Name = " Raichu ",
      Genus = "  Mouse  ",
      Description = "   When its electricity builds, its muscles are stimulated, and it becomes more aggressive than usual.   ",
      GenderRatio = 4,
      CanChangeForm = true,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "    This is the default variety.    "
    };
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderPunch.EntityId.ToString().ToUpperInvariant()}  ", 0));
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderShock.Key.Value.ToUpperInvariant()}  ", 1));

    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Variety);

    VarietyModel variety = result.Variety;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, variety.Id);
    }
    else
    {
      Assert.NotEqual(default, variety.Id);
    }
    Assert.Equal(4, variety.Version);
    Assert.Equal(Actor, variety.CreatedBy);
    Assert.Equal(DateTime.UtcNow, variety.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, variety.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, variety.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_species.EntityId, variety.Species.Id);
    Assert.Equal(payload.IsDefault, variety.IsDefault);
    Assert.Equal(payload.Key.ToLowerInvariant(), variety.Key);
    Assert.Equal(payload.Name.Trim(), variety.Name);
    Assert.Equal(payload.Genus.Trim(), variety.Genus);
    Assert.Equal(payload.Description.Trim(), variety.Description);
    Assert.Equal(payload.GenderRatio, variety.GenderRatio);
    Assert.Equal(payload.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(payload.Url, variety.Url);
    Assert.Equal(payload.Notes.Trim(), variety.Notes);

    Assert.Equal(2, variety.Moves.Count);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderPunch.EntityId && x.Level == 0);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderShock.EntityId && x.Level == 1);
  }

  [Fact(DisplayName = "It should read a variety by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _variety.EntityId;
    VarietyModel? variety = await _varietyService.ReadAsync(id);
    Assert.NotNull(variety);
    Assert.Equal(id, variety.Id);
  }

  [Fact(DisplayName = "It should read a variety by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    VarietyModel? variety = await _varietyService.ReadAsync(id: null, $" {_variety.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(variety);
    Assert.Equal(_variety.EntityId, variety.Id);
  }

  [Fact(DisplayName = "It should replace an existing variety.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    _variety.SetLevelMove(_thunderPunch, new Level(1), World.OwnerId);
    _variety.SetEvolutionMove(_agility, World.OwnerId);
    await _varietyRepository.SaveAsync(_variety);

    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = _species.EntityId.ToString(),
      IsDefault = true,
      Key = "raichu",
      Name = " Raichu ",
      Genus = "  Mouse  ",
      Description = "   When its electricity builds, its muscles are stimulated, and it becomes more aggressive than usual.   ",
      GenderRatio = 4,
      CanChangeForm = true,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "    This is the default variety.    "
    };
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderPunch.EntityId.ToString().ToUpperInvariant()}  ", 0));
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderShock.Key.Value.ToUpperInvariant()}  ", 1));
    Guid id = _variety.EntityId;

    CreateOrReplaceVarietyResult result = await _varietyService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Variety);

    VarietyModel variety = result.Variety;
    Assert.Equal(id, variety.Id);
    Assert.Equal(9, variety.Version);
    Assert.Equal(Actor, variety.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, variety.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_species.EntityId, variety.Species.Id);
    Assert.Equal(payload.IsDefault, variety.IsDefault);
    Assert.Equal(payload.Key.ToLowerInvariant(), variety.Key);
    Assert.Equal(payload.Name.Trim(), variety.Name);
    Assert.Equal(payload.Genus.Trim(), variety.Genus);
    Assert.Equal(payload.Description.Trim(), variety.Description);
    Assert.Equal(payload.GenderRatio, variety.GenderRatio);
    Assert.Equal(payload.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(payload.Url, variety.Url);
    Assert.Equal(payload.Notes.Trim(), variety.Notes);

    Assert.Equal(2, variety.Moves.Count);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderPunch.EntityId && x.Level == 0);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderShock.EntityId && x.Level == 1);
  }

  [Fact(DisplayName = "It should return the correct search results (CanChangeForm).")]
  public async Task Given_CanChangeForm_When_Search_Then_Results()
  {
    PokemonSpecies eeveeSpecies = SpeciesBuilder.Eevee(Faker, World);
    PokemonSpecies darmanitanSpecies = SpeciesBuilder.Darmanitan(Faker, World);
    await _speciesRepository.SaveAsync([eeveeSpecies, darmanitanSpecies]);

    Variety eevee = VarietyBuilder.Eevee(Faker, World, eeveeSpecies);
    Variety darmanitan = VarietyBuilder.Darmanitan(Faker, World, darmanitanSpecies);
    await _varietyRepository.SaveAsync([eevee, darmanitan]);

    SearchVarietiesPayload payload = new()
    {
      Ids = [eevee.EntityId, darmanitan.EntityId],
      CanChangeForm = Faker.Random.Bool()
    };

    SearchResults<VarietyModel> results = await _varietyService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    VarietyModel variety = Assert.Single(results.Items);
    Assert.Equal((payload.CanChangeForm.Value ? darmanitan : eevee).EntityId, variety.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (SpeciesId).")]
  public async Task Given_SpeciesId_When_Search_Then_Results()
  {
    PokemonSpecies eeveeSpecies = SpeciesBuilder.Eevee(Faker, World);
    PokemonSpecies darmanitanSpecies = SpeciesBuilder.Darmanitan(Faker, World);
    await _speciesRepository.SaveAsync([eeveeSpecies, darmanitanSpecies]);

    Variety eevee = VarietyBuilder.Eevee(Faker, World, eeveeSpecies);
    Variety darmanitan = VarietyBuilder.Darmanitan(Faker, World, darmanitanSpecies);
    Variety darmanitanGalar = VarietyBuilder.DarmanitanGalar(Faker, World, darmanitanSpecies);
    await _varietyRepository.SaveAsync([eevee, darmanitan, darmanitanGalar]);

    SearchVarietiesPayload payload = new()
    {
      Ids = [eevee.EntityId, darmanitan.EntityId, darmanitanGalar.EntityId],
      SpeciesId = Faker.PickRandom(eeveeSpecies.EntityId, darmanitanSpecies.EntityId)
    };
    SearchResults<VarietyModel> results = await _varietyService.SearchAsync(payload);

    if (payload.SpeciesId == eeveeSpecies.EntityId)
    {
      Assert.Equal(1, results.Total);
      Assert.Equal(eevee.EntityId, Assert.Single(results.Items).Id);
    }
    else
    {
      Assert.Equal(2, results.Total);
      Assert.Equal(results.Total, results.Items.Count);
      Assert.Contains(results.Items, x => x.Id == darmanitan.EntityId);
      Assert.Contains(results.Items, x => x.Id == darmanitanGalar.EntityId);
    }
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_Search_Then_Results()
  {
    PokemonSpecies eeveeSpecies = SpeciesBuilder.Eevee(Faker, World);
    PokemonSpecies pichuSpecies = SpeciesBuilder.Pichu(Faker, World);
    PokemonSpecies pikachuSpecies = SpeciesBuilder.Pikachu(Faker, World);
    await _speciesRepository.SaveAsync([eeveeSpecies, pichuSpecies, pikachuSpecies]);

    Variety eevee = VarietyBuilder.Eevee(Faker, World, eeveeSpecies);
    Variety pichu = VarietyBuilder.Pichu(Faker, World, pichuSpecies);
    Variety pikachu = VarietyBuilder.Pikachu(Faker, World, pikachuSpecies);
    await _varietyRepository.SaveAsync([eevee, pichu, pikachu]);

    SearchVarietiesPayload payload = new()
    {
      Ids = [eevee.EntityId, pichu.EntityId, pikachu.EntityId, Guid.Empty],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("%chu"));
    payload.Sort.Add(new VarietySortOption(VarietySort.Key, isDescending: true));

    SearchResults<VarietyModel> results = await _varietyService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    VarietyModel model = Assert.Single(results.Items);
    Assert.Equal(pichu.EntityId, model.Id);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = _species.Key.Value,
      Key = _variety.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _varietyService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Variety", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_variety.EntityId, exception.ConflictId);
    Assert.Equal(_variety.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing variety.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    _variety.SetLevelMove(_agility, new Level(1), World.OwnerId);
    _variety.SetLevelMove(_quickAttack, new Level(1), World.OwnerId);
    _variety.SetLevelMove(_thunderPunch, new Level(1), World.OwnerId);
    await _varietyRepository.SaveAsync(_variety);

    Guid id = _variety.EntityId;
    UpdateVarietyPayload payload = new()
    {
      IsDefault = true,
      Key = "raichu",
      Name = new Optional<string>(" Raichu "),
      Genus = new Optional<string>("  Mouse  "),
      Description = new Optional<string>("   When its electricity builds, its muscles are stimulated, and it becomes more aggressive than usual.   "),
      GenderRatio = new Optional<int?>(4),
      CanChangeForm = true,
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("    This is the default variety.    ")
    };
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderPunch.EntityId.ToString().ToUpperInvariant()}  ", 0));
    payload.Moves.Add(new VarietyMovePayload($"  {_thunderShock.Key.Value.ToUpperInvariant()}  ", 1));
    payload.Moves.Add(new VarietyMovePayload(_agility.EntityId.ToString()));

    VarietyModel? variety = await _varietyService.UpdateAsync(id, payload);
    Assert.NotNull(variety);

    Assert.Equal(id, variety.Id);
    Assert.Equal(10, variety.Version);
    Assert.Equal(Actor, variety.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, variety.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_species.EntityId, variety.Species.Id);
    Assert.Equal(payload.IsDefault, variety.IsDefault);
    Assert.Equal(payload.Key.ToLowerInvariant(), variety.Key);
    Assert.Equal(payload.Name.Value?.Trim(), variety.Name);
    Assert.Equal(payload.Genus.Value?.Trim(), variety.Genus);
    Assert.Equal(payload.Description.Value?.Trim(), variety.Description);
    Assert.Equal(payload.GenderRatio.Value, variety.GenderRatio);
    Assert.Equal(payload.CanChangeForm, variety.CanChangeForm);
    Assert.Equal(payload.Url.Value, variety.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), variety.Notes);

    Assert.Equal(3, variety.Moves.Count);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderPunch.EntityId && x.Level == 0);
    Assert.Contains(variety.Moves, x => x.Move.Id == _thunderShock.EntityId && x.Level == 1);
    Assert.Contains(variety.Moves, x => x.Move.Id == _quickAttack.EntityId && x.Level == 1);
  }
}
