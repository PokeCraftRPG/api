using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Abilities.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateAbilityCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IAbilityQuerier> _abilityQuerier = new();
  private readonly Mock<IAbilityRepository> _abilityRepository = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateAbilityCommandHandler _handler;

  public UpdateAbilityCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_abilityQuerier.Object, _abilityRepository.Object, _context, _permissionService.Object, _storageService.Object);
  }

  [Fact(DisplayName = "It should return null when the ability does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateAbilityPayload payload = new();
    UpdateAbilityCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateAbilityPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Url = new Optional<string>("invalid")
    };
    UpdateAbilityCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing ability.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Ability ability = new AbilityBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _abilityRepository.Setup(x => x.LoadAsync(ability.Id, _cancellationToken)).ReturnsAsync(ability);

    UpdateAbilityPayload payload = new()
    {
      Key = "static",
      Name = new Optional<string>("Static"),
      Description = new Optional<string>("The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it."),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)"),
      Notes = new Optional<string>("When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves).")
    };
    UpdateAbilityCommand command = new(ability.EntityId, payload);

    AbilityModel model = new();
    _abilityQuerier.Setup(x => x.ReadAsync(ability, _cancellationToken)).ReturnsAsync(model);

    AbilityModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, ability, _cancellationToken), Times.Once());
    _abilityQuerier.Verify(x => x.EnsureUnicityAsync(ability, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(ability, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
