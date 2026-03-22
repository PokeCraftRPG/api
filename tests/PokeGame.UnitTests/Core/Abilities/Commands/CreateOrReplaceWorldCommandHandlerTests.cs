using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Abilities.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Abilities.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceAbilityCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IAbilityQuerier> _abilityQuerier = new();
  private readonly Mock<IAbilityRepository> _abilityRepository = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceAbilityCommandHandler _handler;

  public CreateOrReplaceAbilityCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_abilityQuerier.Object, _abilityRepository.Object, _context, _permissionService.Object, _storageService.Object);
  }

  [Theory(DisplayName = "It should create a new ability.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "static",
      Name = "Static",
      Description = "The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)",
      Notes = "When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves)."
    };
    CreateOrReplaceAbilityCommand command = new(payload, id);

    AbilityModel model = new();
    _abilityQuerier.Setup(x => x.ReadAsync(It.IsAny<Ability>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceAbilityResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Ability);

    if (id.HasValue)
    {
      AbilityId abilityId = new(_context.WorldId, id.Value);
      _abilityRepository.Verify(x => x.LoadAsync(abilityId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateAbility, _cancellationToken), Times.Once());
    _abilityQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Ability>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Ability>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing ability.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Ability ability = new AbilityBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _abilityRepository.Setup(x => x.LoadAsync(ability.Id, _cancellationToken)).ReturnsAsync(ability);

    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "static",
      Name = "Static",
      Description = "The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)",
      Notes = "When hit by a contact move, attacker has a 30% chance to be paralyzed (check per hit for multi-strike moves)."
    };
    CreateOrReplaceAbilityCommand command = new(payload, ability.EntityId);

    AbilityModel model = new();
    _abilityQuerier.Setup(x => x.ReadAsync(ability, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceAbilityResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Ability);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, ability, _cancellationToken), Times.Once());
    _abilityQuerier.Verify(x => x.EnsureUnicityAsync(ability, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(ability, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceAbilityPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Url = "invalid"
    };
    CreateOrReplaceAbilityCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
