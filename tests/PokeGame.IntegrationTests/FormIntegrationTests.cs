using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class FormIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IFormService _formService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _lightningRod = null!;
  private Ability _static = null!;
  private Ability _surgeSurfer = null!;
  private SpeciesAggregate _species = null!;
  private Variety _variety = null!;
  private Form _form = null!;

  public FormIntegrationTests() : base()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _formService = ServiceProvider.GetRequiredService<IFormService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _lightningRod = AbilityBuilder.LightningRod(Faker, World);
    _static = AbilityBuilder.Static(Faker, World);
    _surgeSurfer = AbilityBuilder.SurgeSurfer(Faker, World);
    await _abilityRepository.SaveAsync([_lightningRod, _static, _surgeSurfer]);

    _species = SpeciesBuilder.Raichu(Faker, World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Raichu(Faker, World, _species);
    await _varietyRepository.SaveAsync(_variety);

    _form = new FormBuilder(Faker).WithWorld(World).WithVariety(_variety).Build();
    await _formRepository.SaveAsync(_form);
  }

  [Theory(DisplayName = "It should create a new form.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceFormPayload payload = new()
    {
      Variety = "  rAIchU  ",
      Key = "raichu-alola",
      Name = " Alolan Raichu ",
      Description = "  Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.  ",
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload($"  {_surgeSurfer.Key.Value.ToUpperInvariant()}  ", secondary: null, $"  {_lightningRod.EntityId.ToString().ToUpperInvariant()}  "),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "   Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks.   "
    };

    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Form);

    FormModel form = result.Form;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, form.Id);
    }
    else
    {
      Assert.NotEqual(default, form.Id);
    }
    Assert.Equal(2, form.Version);
    Assert.Equal(Actor, form.CreatedBy);
    Assert.Equal(DateTime.UtcNow, form.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, form.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, form.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_variety.EntityId, form.Variety.Id);
    Assert.Equal(payload.IsDefault, form.IsDefault);
    Assert.Equal(payload.Key.ToLowerInvariant(), form.Key);
    Assert.Equal(payload.Name.Trim(), form.Name);
    Assert.Equal(payload.Description.Trim(), form.Description);
    Assert.Equal(payload.IsBattleOnly, form.IsBattleOnly);
    Assert.Equal(payload.IsMega, form.IsMega);
    Assert.Equal(payload.Height, form.Height);
    Assert.Equal(payload.Weight, form.Weight);
    Assert.Equal(payload.Types, form.Types);
    Assert.Equal(payload.BaseStatistics, form.BaseStatistics);
    Assert.Equal(payload.Yield, form.Yield);
    Assert.Equal(payload.Sprites, form.Sprites);
    Assert.Equal(payload.Url, form.Url);
    Assert.Equal(payload.Notes.Trim(), form.Notes);

    Assert.Equal(_surgeSurfer.EntityId, form.Abilities.Primary.Id);
    Assert.Null(form.Abilities.Secondary);
    Assert.NotNull(form.Abilities.Hidden);
    Assert.Equal(_lightningRod.EntityId, form.Abilities.Hidden.Id);
  }

  [Fact(DisplayName = "It should read a form by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _form.EntityId;
    FormModel? form = await _formService.ReadAsync(id);
    Assert.NotNull(form);
    Assert.Equal(id, form.Id);
  }

  [Fact(DisplayName = "It should read a form by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    FormModel? form = await _formService.ReadAsync(id: null, $" {_form.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(form);
    Assert.Equal(_form.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should replace an existing form.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceFormPayload payload = new()
    {
      Variety = "  rAIchU  ",
      Key = "raichu-alola",
      Name = " Alolan Raichu ",
      Description = "  Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.  ",
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload($"  {_surgeSurfer.Key.Value.ToUpperInvariant()}  ", secondary: null, $"  {_lightningRod.EntityId.ToString().ToUpperInvariant()}  "),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "   Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks.   "
    };
    Guid id = _form.EntityId;

    CreateOrReplaceFormResult result = await _formService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Form);

    FormModel form = result.Form;
    Assert.Equal(id, form.Id);
    Assert.Equal(3, form.Version);
    Assert.Equal(Actor, form.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, form.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_variety.EntityId, form.Variety.Id);
    Assert.Equal(payload.IsDefault, form.IsDefault);
    Assert.Equal(payload.Key.ToLowerInvariant(), form.Key);
    Assert.Equal(payload.Name.Trim(), form.Name);
    Assert.Equal(payload.Description.Trim(), form.Description);
    Assert.Equal(payload.IsBattleOnly, form.IsBattleOnly);
    Assert.Equal(payload.IsMega, form.IsMega);
    Assert.Equal(payload.Height, form.Height);
    Assert.Equal(payload.Weight, form.Weight);
    Assert.Equal(payload.Types, form.Types);
    Assert.Equal(payload.BaseStatistics, form.BaseStatistics);
    Assert.Equal(payload.Yield, form.Yield);
    Assert.Equal(payload.Sprites, form.Sprites);
    Assert.Equal(payload.Url, form.Url);
    Assert.Equal(payload.Notes.Trim(), form.Notes);

    Assert.Equal(_surgeSurfer.EntityId, form.Abilities.Primary.Id);
    Assert.Null(form.Abilities.Secondary);
    Assert.NotNull(form.Abilities.Hidden);
    Assert.Equal(_lightningRod.EntityId, form.Abilities.Hidden.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (AbilityId).")]
  public async Task Given_AbilityId_When_Search_Then_Results()
  {
    Form raichu = FormBuilder.Raichu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _variety, new FormAbilities(_surgeSurfer, secondary: null, _lightningRod));
    await _formRepository.SaveAsync([raichu, raichuAlola]);

    SearchFormsPayload payload = new()
    {
      Ids = [raichu.EntityId, raichuAlola.EntityId],
      AbilityId = Faker.PickRandom(_lightningRod.EntityId, _static.EntityId, _surgeSurfer.EntityId)
    };
    SearchResults<FormModel> results = await _formService.SearchAsync(payload);

    if (payload.AbilityId == _static.EntityId)
    {
      Assert.Equal(1, results.Total);
      Assert.Equal(raichu.EntityId, Assert.Single(results.Items).Id);
    }
    else if (payload.AbilityId == _surgeSurfer.EntityId)
    {
      Assert.Equal(1, results.Total);
      Assert.Equal(raichuAlola.EntityId, Assert.Single(results.Items).Id);
    }
    else
    {
      Assert.Equal(2, results.Total);
      Assert.Equal(results.Total, results.Items.Count);
      Assert.Contains(results.Items, x => x.Id == raichu.EntityId);
      Assert.Contains(results.Items, x => x.Id == raichuAlola.EntityId);
    }
  }

  [Fact(DisplayName = "It should return the correct search results (IsBattleOnly).")]
  public async Task Given_IsBattleOnly_When_Search_Then_Results()
  {
    Form raichu = FormBuilder.Raichu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form battle = new FormBuilder(Faker).WithWorld(World).WithVariety(_variety).WithKey(new Slug("raichu-battle")).IsBattleOnly().Build();
    await _formRepository.SaveAsync([raichu, battle]);

    SearchFormsPayload payload = new()
    {
      Ids = [raichu.EntityId, battle.EntityId],
      IsBattleOnly = Faker.Random.Bool()
    };

    SearchResults<FormModel> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormModel form = Assert.Single(results.Items);
    Assert.Equal((payload.IsBattleOnly.Value ? battle : raichu).EntityId, form.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (IsMega).")]
  public async Task Given_IsMega_When_Search_Then_Results()
  {
    Form raichu = FormBuilder.Raichu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form mega = new FormBuilder(Faker).WithWorld(World).WithVariety(_variety).WithKey(new Slug("raichu-mega")).IsMega().Build();
    await _formRepository.SaveAsync([raichu, mega]);

    SearchFormsPayload payload = new()
    {
      Ids = [raichu.EntityId, mega.EntityId],
      IsMega = Faker.Random.Bool()
    };

    SearchResults<FormModel> results = await _formService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    FormModel form = Assert.Single(results.Items);
    Assert.Equal((payload.IsMega.Value ? mega : raichu).EntityId, form.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Type).")]
  public async Task Given_Type_When_Search_Then_Results()
  {
    Form raichu = FormBuilder.Raichu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _variety, new FormAbilities(_surgeSurfer, secondary: null, _lightningRod));
    await _formRepository.SaveAsync([raichu, raichuAlola]);

    SearchFormsPayload payload = new()
    {
      Ids = [raichu.EntityId, raichuAlola.EntityId],
      Type = Faker.PickRandom(PokemonType.Normal, PokemonType.Electric, PokemonType.Fairy)
    };
    SearchResults<FormModel> results = await _formService.SearchAsync(payload);

    switch (payload.Type)
    {
      case PokemonType.Electric:
        Assert.Equal(2, results.Total);
        Assert.Equal(results.Total, results.Items.Count);
        Assert.Contains(results.Items, x => x.Id == raichu.EntityId);
        Assert.Contains(results.Items, x => x.Id == raichuAlola.EntityId);
        break;
      case PokemonType.Fairy:
        Assert.Equal(1, results.Total);
        Assert.Equal(raichuAlola.EntityId, Assert.Single(results.Items).Id);
        break;
      default:
        Assert.Equal(0, results.Total);
        Assert.Empty(results.Items);
        break;
    }
  }

  [Fact(DisplayName = "It should return the correct search results (VarietyId).")]
  public async Task Given_VarietyId_When_Search_Then_Results()
  {
    SpeciesAggregate pikachuSpecies = SpeciesBuilder.Pikachu(Faker, World);
    await _speciesRepository.SaveAsync(pikachuSpecies);

    Variety pikachuVariety = VarietyBuilder.Pikachu(Faker, World, pikachuSpecies);
    await _varietyRepository.SaveAsync(pikachuVariety);

    Form pikachu = FormBuilder.Pikachu(Faker, World, pikachuVariety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form raichu = FormBuilder.Raichu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _variety, new FormAbilities(_surgeSurfer, secondary: null, _lightningRod));
    await _formRepository.SaveAsync([pikachu, raichu, raichuAlola]);

    SearchFormsPayload payload = new()
    {
      Ids = [pikachu.EntityId, raichu.EntityId, raichuAlola.EntityId],
      VarietyId = Faker.PickRandom(pikachuVariety.EntityId, _variety.EntityId)
    };
    SearchResults<FormModel> results = await _formService.SearchAsync(payload);

    if (payload.VarietyId == pikachuVariety.EntityId)
    {
      Assert.Equal(1, results.Total);
      Assert.Equal(pikachu.EntityId, Assert.Single(results.Items).Id);
    }
    else
    {
      Assert.Equal(2, results.Total);
      Assert.Equal(results.Total, results.Items.Count);
      Assert.Contains(results.Items, x => x.Id == raichu.EntityId);
      Assert.Contains(results.Items, x => x.Id == raichuAlola.EntityId);
    }
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_Search_Then_Results()
  {
    SpeciesAggregate pichuSpecies = SpeciesBuilder.Pichu(Faker, World);
    SpeciesAggregate pikachuSpecies = SpeciesBuilder.Pikachu(Faker, World);
    await _speciesRepository.SaveAsync([pichuSpecies, pikachuSpecies]);

    Variety pichuVariety = VarietyBuilder.Pichu(Faker, World, pichuSpecies);
    Variety pikachuVariety = VarietyBuilder.Pikachu(Faker, World, pikachuSpecies);
    await _varietyRepository.SaveAsync([pichuVariety, pikachuVariety]);

    Form pichu = FormBuilder.Pichu(Faker, World, pichuVariety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form pikachu = FormBuilder.Pikachu(Faker, World, pikachuVariety, new FormAbilities(_static, secondary: null, _lightningRod));
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _variety, new FormAbilities(_surgeSurfer, secondary: null, _lightningRod));
    await _formRepository.SaveAsync([pichu, pikachu, raichuAlola]);

    SearchFormsPayload payload = new()
    {
      Ids = [_form.EntityId, pichu.EntityId, Guid.Empty, raichuAlola.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("%chu"));
    payload.Sort.Add(new FormSortOption(FormSort.Name, isDescending: true));

    SearchResults<FormModel> results = await _formService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    FormModel form = Assert.Single(results.Items);
    Assert.Equal(raichuAlola.EntityId, form.Id);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceFormPayload payload = new()
    {
      Variety = _variety.EntityId.ToString(),
      Key = _form.Key.Value.ToUpperInvariant(),
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload(_surgeSurfer.EntityId.ToString(), secondary: null, _lightningRod.EntityId.ToString()),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _formService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Form", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_form.EntityId, exception.ConflictId);
    Assert.Equal(_form.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing form.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _form.EntityId;
    UpdateFormPayload payload = new()
    {
      IsDefault = true,
      Name = new Optional<string>(" Alolan Raichu "),
      Description = new Optional<string>("  Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.  "),
      IsBattleOnly = true,
      IsMega = true,
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload($"  {_surgeSurfer.Key.Value.ToUpperInvariant()}  ", secondary: null, $"  {_lightningRod.EntityId.ToString().ToUpperInvariant()}  "),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("   Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks.   ")
    };

    FormModel? form = await _formService.UpdateAsync(id, payload);
    Assert.NotNull(form);

    Assert.Equal(id, form.Id);
    Assert.Equal(3, form.Version);
    Assert.Equal(Actor, form.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, form.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_variety.EntityId, form.Variety.Id);
    Assert.Equal(payload.IsDefault, form.IsDefault);
    Assert.Equal(_form.Key.Value, form.Key);
    Assert.Equal(payload.Name.Value?.Trim(), form.Name);
    Assert.Equal(payload.Description.Value?.Trim(), form.Description);
    Assert.Equal(payload.IsBattleOnly, form.IsBattleOnly);
    Assert.Equal(payload.IsMega, form.IsMega);
    Assert.Equal(payload.Height, form.Height);
    Assert.Equal(payload.Weight, form.Weight);
    Assert.Equal(payload.Types, form.Types);
    Assert.Equal(payload.BaseStatistics, form.BaseStatistics);
    Assert.Equal(payload.Yield, form.Yield);
    Assert.Equal(payload.Sprites, form.Sprites);
    Assert.Equal(payload.Url.Value, form.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), form.Notes);

    Assert.Equal(_surgeSurfer.EntityId, form.Abilities.Primary.Id);
    Assert.Null(form.Abilities.Secondary);
    Assert.NotNull(form.Abilities.Hidden);
    Assert.Equal(_lightningRod.EntityId, form.Abilities.Hidden.Id);
  }
}
