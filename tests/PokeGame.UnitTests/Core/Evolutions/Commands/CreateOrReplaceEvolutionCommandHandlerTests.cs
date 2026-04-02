using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceEvolutionCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IEvolutionQuerier> _evolutionQuerier = new();
  private readonly Mock<IEvolutionRepository> _evolutionRepository = new();
  private readonly Mock<IFormManager> _formManager = new();
  private readonly Mock<IItemManager> _itemManager = new();
  private readonly Mock<IMoveManager> _moveManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceEvolutionCommandHandler _handler;

  private readonly World _world;
  private readonly Form _pikachu;
  private readonly Form _raichu;
  private readonly Item _thunderStone;
  private readonly Item _oranBerry;
  private readonly Move _thunderPunch;

  public CreateOrReplaceEvolutionCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
      _evolutionQuerier.Object,
      _evolutionRepository.Object,
      _formManager.Object,
      _itemManager.Object,
      _moveManager.Object,
      _permissionService.Object,
      _storageService.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;

    _pikachu = FormBuilder.Pikachu(_faker, _world);
    _formManager.Setup(x => x.FindAsync(_pikachu.Key.Value, It.IsAny<string>(), _cancellationToken)).ReturnsAsync(_pikachu);

    _raichu = FormBuilder.Raichu(_faker, _world);
    _formManager.Setup(x => x.FindAsync(_raichu.Key.Value, It.IsAny<string>(), _cancellationToken)).ReturnsAsync(_raichu);

    _thunderStone = ItemBuilder.ThunderStone(_faker, _world);
    _itemManager.Setup(x => x.FindAsync(_thunderStone.Key.Value, It.IsAny<string>(), _cancellationToken)).ReturnsAsync(_thunderStone);

    _oranBerry = ItemBuilder.OranBerry(_faker, _world);
    _itemManager.Setup(x => x.FindAsync(_oranBerry.Key.Value, It.IsAny<string>(), _cancellationToken)).ReturnsAsync(_oranBerry);

    _thunderPunch = MoveBuilder.ThunderPunch(_faker, _world);
    _moveManager.Setup(x => x.FindAsync(_thunderPunch.Key.Value, It.IsAny<string>(), _cancellationToken)).ReturnsAsync(_thunderPunch);
  }

  [Theory(DisplayName = "It should create a new evolution.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pikachu",
      Target = "raichu",
      Trigger = EvolutionTrigger.Item,
      Item = "thunder-stone",
      HeldItem = "oran-berry",
      KnownMove = "thunder-punch"
    };
    CreateOrReplaceEvolutionCommand command = new(payload, id);

    EvolutionModel model = new();
    _evolutionQuerier.Setup(x => x.ReadAsync(It.IsAny<Evolution>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceEvolutionResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Evolution);

    if (id.HasValue)
    {
      EvolutionId evolutionId = new(_context.WorldId, id.Value);
      _evolutionRepository.Verify(x => x.LoadAsync(evolutionId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateEvolution, _cancellationToken), Times.Once());
    _evolutionQuerier.Verify(x => x.EnsureDifferentSpeciesAsync(
      It.Is<IEnumerable<Form>>(y => y.SequenceEqual(new Form[] { _pikachu, _raichu })),
      _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Evolution>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing evolution.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world, _pikachu, _raichu, _thunderStone);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pikachu",
      Target = "raichu",
      Trigger = EvolutionTrigger.Item,
      Item = "thunder-stone",
      HeldItem = "oran-berry",
      KnownMove = "thunder-punch"
    };
    CreateOrReplaceEvolutionCommand command = new(payload, evolution.EntityId);

    EvolutionModel model = new();
    _evolutionQuerier.Setup(x => x.ReadAsync(evolution, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceEvolutionResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Evolution);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, evolution, _cancellationToken), Times.Once());
    _evolutionQuerier.Verify(x => x.EnsureDifferentSpeciesAsync(
      It.Is<IEnumerable<Form>>(y => y.SequenceEqual(new Form[] { _pikachu, _raichu })),
      _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(evolution, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());

    Assert.Equal(_oranBerry.Id, evolution.HeldItemId);
    Assert.Equal(_thunderPunch.Id, evolution.KnownMoveId);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the item has changed.")]
  public async Task Given_ItemChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world, _pikachu, _raichu, _thunderStone);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pikachu",
      Target = "raichu",
      Trigger = EvolutionTrigger.Item,
      Item = "oran-berry"
    };
    CreateOrReplaceEvolutionCommand command = new(payload, evolution.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid?>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(evolution.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Evolution", exception.EntityKind);
    Assert.Equal(evolution.EntityId, exception.EntityId);
    Assert.Equal(evolution.ItemId?.EntityId, exception.ExpectedValue);
    Assert.Equal(_oranBerry.EntityId, exception.AttemptedValue);
    Assert.Equal("Item", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the source has changed.")]
  public async Task Given_SourceChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world, _pikachu, _raichu, _thunderStone);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pichu",
      Target = "raichu",
      Trigger = EvolutionTrigger.Item,
      Item = "thunder-stone"
    };
    CreateOrReplaceEvolutionCommand command = new(payload, evolution.EntityId);

    Form pichu = FormBuilder.Pichu(_faker, _world);
    _formManager.Setup(x => x.FindAsync(payload.Source, nameof(payload.Source), _cancellationToken)).ReturnsAsync(pichu);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(evolution.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Evolution", exception.EntityKind);
    Assert.Equal(evolution.EntityId, exception.EntityId);
    Assert.Equal(evolution.SourceId.EntityId, exception.ExpectedValue);
    Assert.Equal(pichu.EntityId, exception.AttemptedValue);
    Assert.Equal("Source", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the target has changed.")]
  public async Task Given_TargetChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world, _pikachu, _raichu, _thunderStone);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pikachu",
      Target = "pichu",
      Trigger = EvolutionTrigger.Level,
      Friendship = true
    };
    CreateOrReplaceEvolutionCommand command = new(payload, evolution.EntityId);

    Form pichu = FormBuilder.Pichu(_faker, _world);
    _formManager.Setup(x => x.FindAsync(payload.Target, nameof(payload.Target), _cancellationToken)).ReturnsAsync(pichu);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(evolution.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Evolution", exception.EntityKind);
    Assert.Equal(evolution.EntityId, exception.EntityId);
    Assert.Equal(evolution.TargetId.EntityId, exception.ExpectedValue);
    Assert.Equal(pichu.EntityId, exception.AttemptedValue);
    Assert.Equal("Target", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the trigger has changed.")]
  public async Task Given_TriggerChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world, _pikachu, _raichu, _thunderStone);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = "pikachu",
      Target = "raichu",
      Trigger = EvolutionTrigger.Trade
    };
    CreateOrReplaceEvolutionCommand command = new(payload, evolution.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<EvolutionTrigger>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(evolution.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Evolution", exception.EntityKind);
    Assert.Equal(evolution.EntityId, exception.EntityId);
    Assert.Equal(evolution.Trigger, exception.ExpectedValue);
    Assert.Equal(payload.Trigger, exception.AttemptedValue);
    Assert.Equal("Trigger", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceEvolutionPayload payload = new()
    {
      Trigger = (EvolutionTrigger)(-1),
      Item = "thunder-stone",
      Level = 0,
      Gender = (PokemonGender)(-1),
      Location = _faker.Random.String(999, 'a', 'z'),
      TimeOfDay = (TimeOfDay)(-1)
    };
    CreateOrReplaceEvolutionCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(8, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Source");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Target");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Trigger");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EmptyValidator" && e.PropertyName == "Item");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Level.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Gender");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Location");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "TimeOfDay");
  }
}
