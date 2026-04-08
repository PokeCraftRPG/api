using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Forms.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceFormCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IFormManager> _formManager = new();
  private readonly Mock<IFormQuerier> _formQuerier = new();
  private readonly Mock<IFormRepository> _formRepository = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IVarietyManager> _varietyManager = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceFormCommandHandler _handler;

  public CreateOrReplaceFormCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _formManager.Object, _formQuerier.Object, _formRepository.Object, _permissionService.Object, _storageService.Object, _varietyManager.Object);
  }

  [Theory(DisplayName = "It should create a new form.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceFormPayload payload = new()
    {
      Variety = "raichu",
      Key = "raichu-alola",
      Name = "Alolan Raichu",
      Description = "Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.",
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload("surge-surfer", secondary: null, Guid.NewGuid().ToString()),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks."
    };
    CreateOrReplaceFormCommand command = new(payload, id);

    Variety variety = VarietyBuilder.Raichu(_faker, _context.World);
    _varietyManager.Setup(x => x.FindAsync(payload.Variety, nameof(payload.Variety), _cancellationToken)).ReturnsAsync(variety);

    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _context.World);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _context.World);
    FormAbilities abilities = new(surgeSurfer, secondary: null, lightningRod);
    _formManager.Setup(x => x.FindAbilitiesAsync(payload.Abilities, nameof(payload.Abilities), _cancellationToken)).ReturnsAsync(abilities);

    FormModel model = new();
    _formQuerier.Setup(x => x.ReadAsync(It.IsAny<Form>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceFormResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Form);

    if (id.HasValue)
    {
      FormId formId = new(_context.WorldId, id.Value);
      _formRepository.Verify(x => x.LoadAsync(formId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateForm, _cancellationToken), Times.Once());
    _formQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Form>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Form>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing form.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Variety variety = VarietyBuilder.Raichu(_faker, _context.World);
    Form form = new FormBuilder(_faker).WithWorld(_context.World).WithVariety(variety).ClearChanges().Build();
    _formRepository.Setup(x => x.LoadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);

    CreateOrReplaceFormPayload payload = new()
    {
      Variety = "raichu",
      Key = "raichu-alola",
      Name = "Alolan Raichu",
      Description = "Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.",
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload("surge-surfer", secondary: null, Guid.NewGuid().ToString()),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks."
    };
    CreateOrReplaceFormCommand command = new(payload, form.EntityId);

    _varietyManager.Setup(x => x.FindAsync(payload.Variety, nameof(payload.Variety), _cancellationToken)).ReturnsAsync(variety);

    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _context.World);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _context.World);
    FormAbilities abilities = new(surgeSurfer, secondary: null, lightningRod);
    _formManager.Setup(x => x.FindAbilitiesAsync(payload.Abilities, nameof(payload.Abilities), _cancellationToken)).ReturnsAsync(abilities);

    FormModel model = new();
    _formQuerier.Setup(x => x.ReadAsync(form, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceFormResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Form);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, form, _cancellationToken), Times.Once());
    _formQuerier.Verify(x => x.EnsureUnicityAsync(form, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(form, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the variety has changed.")]
  public async Task Given_VarietyChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Form form = new FormBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _formRepository.Setup(x => x.LoadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);

    CreateOrReplaceFormPayload payload = new()
    {
      Variety = "raichu",
      Key = "raichu-alola",
      Name = "Alolan Raichu",
      Description = "Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries.",
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload("surge-surfer", secondary: null, Guid.NewGuid().ToString()),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)",
      Notes = "Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks."
    };
    CreateOrReplaceFormCommand command = new(payload, form.EntityId);

    Variety variety = VarietyBuilder.Raichu(_faker, _context.World);
    _varietyManager.Setup(x => x.FindAsync(payload.Variety, nameof(payload.Variety), _cancellationToken)).ReturnsAsync(variety);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(Form.EntityKind, exception.EntityKind);
    Assert.Equal(form.EntityId, exception.EntityId);
    Assert.Equal(form.VarietyId.EntityId, exception.ExpectedValue);
    Assert.Equal(variety.EntityId, exception.AttemptedValue);
    Assert.Equal("Variety", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceFormPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Types = new TypesModel(PokemonType.Electric, PokemonType.Electric),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png"),
      Url = "invalid"
    };
    CreateOrReplaceFormCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(18, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Variety");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Height");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Weight");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Types.Secondary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Abilities.Primary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.HP");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.SpecialDefense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Speed");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Yield.Experience");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "YieldValidator" && e.PropertyName == "Yield");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Sprites.Default");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Sprites.Shiny");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
