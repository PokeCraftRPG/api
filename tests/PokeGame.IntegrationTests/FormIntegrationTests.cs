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

  private Ability _surgeSurfer = null!;
  private Ability _lightningRod = null!;
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

    _surgeSurfer = AbilityBuilder.SurgeSurfer(Faker, World);
    _lightningRod = AbilityBuilder.LightningRod(Faker, World);
    await _abilityRepository.SaveAsync([_surgeSurfer, _lightningRod]);

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
