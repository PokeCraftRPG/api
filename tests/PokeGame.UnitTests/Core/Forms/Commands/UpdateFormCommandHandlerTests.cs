using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Forms.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateFormCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IFormManager> _formManager = new();
  private readonly Mock<IFormQuerier> _formQuerier = new();
  private readonly Mock<IFormRepository> _formRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateFormCommandHandler _handler;

  public UpdateFormCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _formManager.Object, _formQuerier.Object, _formRepository.Object, _permissionService.Object, _storageService.Object);
  }

  [Fact(DisplayName = "It should return null when the form does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateFormPayload payload = new();
    UpdateFormCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateFormPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Height = 0,
      Weight = 0,
      Types = new TypesModel((PokemonType)(-1)),
      Abilities = new AbilitiesPayload(),
      BaseStatistics = new BaseStatisticsModel(),
      Yield = new YieldModel(),
      Sprites = new SpritesModel("invalid:default", "invalid:shiny"),
      Url = new Optional<string>("invalid")
    };
    UpdateFormCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(17, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Height.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Weight.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Types.Primary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Abilities.Primary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.HP");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.SpecialDefense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "BaseStatistics.Speed");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Yield.Experience");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "YieldValidator" && e.PropertyName == "Yield");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprites.Default");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprites.Shiny");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing form.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Form form = new FormBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _formRepository.Setup(x => x.LoadAsync(form.Id, _cancellationToken)).ReturnsAsync(form);

    UpdateFormPayload payload = new()
    {
      Key = "raichu-alola",
      Name = new Optional<string>("Alolan Raichu"),
      Description = new Optional<string>("Raichu is a fast Electric Pokémon known for powerful shocks, unique regional forms, and surprising lore details across games and Pokédex entries."),
      Height = 7,
      Weight = 210,
      Types = new TypesModel(PokemonType.Electric, PokemonType.Fairy),
      Abilities = new AbilitiesPayload("surge-surfer", secondary: null, Guid.NewGuid().ToString()),
      BaseStatistics = new BaseStatisticsModel(60, 85, 50, 95, 85, 110),
      Yield = new YieldModel(218, 0, 0, 0, 0, 0, 3),
      Sprites = new SpritesModel("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png", "https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("Raichu offers rich trivia, design history, and variant forms—useful for flavor, worldbuilding, and highlighting inconsistencies or evolution quirks.")
    };
    UpdateFormCommand command = new(form.EntityId, payload);

    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _context.World);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _context.World);
    Abilities abilities = new(surgeSurfer, secondary: null, lightningRod);
    _formManager.Setup(x => x.FindAbilitiesAsync(payload.Abilities, nameof(payload.Abilities), _cancellationToken)).ReturnsAsync(abilities);

    FormModel model = new();
    _formQuerier.Setup(x => x.ReadAsync(form, _cancellationToken)).ReturnsAsync(model);

    FormModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, form, _cancellationToken), Times.Once());
    _formQuerier.Verify(x => x.EnsureUnicityAsync(form, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(form, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
