using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Varieties;

[Trait(Traits.Category, Categories.Integration)]
public class VarietyMoveIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;
  private readonly IVarietyService _varietyService;

  private Move _tackle = null!;
  private Move _ember = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;

  public VarietyMoveIntegrationTests()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
    _varietyService = ServiceProvider.GetRequiredService<IVarietyService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Bulbasaur(Faker, _species, Context.World);
    await _varietyRepository.SaveAsync(_variety);

    _tackle = MoveBuilder.Tackle(Faker, Context.World);
    _ember = MoveBuilder.Ember(Faker, Context.World);
    await _moveRepository.SaveAsync([_tackle, _ember]);
  }

  [Theory(DisplayName = "It should add a variety move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_SetMove_Then_Added(bool withId)
  {
    SetVarietyMovePayload payload = new()
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    VarietyDto variety = await _varietyService.SetMoveAsync(_variety.EntityId, payload, id);
    VarietyMoveDto varietyMove = Assert.Single(variety.Moves);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, varietyMove.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, varietyMove.Id);
    }
    Assert.Equal(_tackle.EntityId, varietyMove.Move.Id);
    Assert.Equal(payload.LearningMethod, varietyMove.LearningMethod);
    Assert.Equal(payload.Level, varietyMove.Level);
    Assert.Equal(Actor, varietyMove.CreatedBy);
    Assert.Equal(DateTime.UtcNow, varietyMove.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(varietyMove.CreatedBy, varietyMove.UpdatedBy);
    Assert.Equal(varietyMove.CreatedOn, varietyMove.UpdatedOn, TimeSpan.FromMilliseconds(1));
  }

  [Fact(DisplayName = "It should update an existing variety move.")]
  public async Task Given_Exists_When_SetMove_Then_Updated()
  {
    VarietyDto created = await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    });
    VarietyMoveDto existing = Assert.Single(created.Moves);

    SetVarietyMovePayload payload = new()
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.Evolution,
      Level = null
    };

    VarietyDto variety = await _varietyService.SetMoveAsync(_variety.EntityId, payload, existing.Id);
    VarietyMoveDto varietyMove = Assert.Single(variety.Moves);

    Assert.Equal(existing.Id, varietyMove.Id);
    Assert.Equal(_tackle.EntityId, varietyMove.Move.Id);
    Assert.Equal(LearningMethod.Evolution, varietyMove.LearningMethod);
    Assert.Null(varietyMove.Level);
    Assert.Equal(existing.CreatedBy, varietyMove.CreatedBy);
    Assert.Equal(existing.CreatedOn, varietyMove.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, varietyMove.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, varietyMove.UpdatedOn, TimeSpan.FromSeconds(10));
  }

  [Fact(DisplayName = "It should remove a variety move.")]
  public async Task Given_Exists_When_RemoveMove_Then_Removed()
  {
    VarietyDto created = await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    });
    VarietyMoveDto existing = Assert.Single(created.Moves);

    VarietyDto? variety = await _varietyService.RemoveMoveAsync(_variety.EntityId, existing.Id);
    Assert.NotNull(variety);
    Assert.Empty(variety.Moves);
  }

  [Fact(DisplayName = "It should read variety moves after they are set.")]
  public async Task Given_Exists_When_Read_Then_MovesRead()
  {
    await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    });
    await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _ember.EntityId,
      LearningMethod = LearningMethod.Reminder,
      Level = null
    });

    VarietyDto? variety = await _varietyService.ReadAsync(_variety.EntityId);
    Assert.NotNull(variety);
    Assert.Equal(2, variety.Moves.Count);
    Assert.Contains(variety.Moves, x => x.Move.Id == _tackle.EntityId && x.LearningMethod == LearningMethod.LevelUp && x.Level == 1);
    Assert.Contains(variety.Moves, x => x.Move.Id == _ember.EntityId && x.LearningMethod == LearningMethod.Reminder && x.Level is null);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when changing the move identifier.")]
  public async Task Given_DifferentMoveId_When_SetMove_Then_ImmutablePropertyException()
  {
    VarietyDto created = await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    });
    VarietyMoveDto existing = Assert.Single(created.Moves);

    SetVarietyMovePayload payload = new()
    {
      MoveId = _ember.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 5
    };

    ImmutablePropertyException<Guid> exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(
      async () => await _varietyService.SetMoveAsync(_variety.EntityId, payload, existing.Id));
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(_variety.EntityId, exception.EntityId);
    Assert.Equal(_tackle.EntityId, exception.ExpectedValue);
    Assert.Equal(_ember.EntityId, exception.AttemptedValue);
    Assert.Equal(nameof(payload.MoveId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the variety does not exist.")]
  public async Task Given_MissingVariety_When_SetMove_Then_EntityNotFoundException()
  {
    Guid missingVarietyId = Guid.NewGuid();
    SetVarietyMovePayload payload = new()
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _varietyService.SetMoveAsync(missingVarietyId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Variety.EntityKind, exception.EntityKind);
    Assert.Equal(missingVarietyId, exception.EntityId);
    Assert.Equal("VarietyId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the move does not exist.")]
  public async Task Given_MissingMove_When_SetMove_Then_EntityNotFoundException()
  {
    Guid missingMoveId = Guid.NewGuid();
    SetVarietyMovePayload payload = new()
    {
      MoveId = missingMoveId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _varietyService.SetMoveAsync(_variety.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(missingMoveId, exception.EntityId);
    Assert.Equal(nameof(payload.MoveId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_SetMove_Then_ValidationException()
  {
    SetVarietyMovePayload payload = new()
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = null
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _varietyService.SetMoveAsync(_variety.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when setting a variety move.")]
  public async Task Given_NotAllowed_When_SetMove_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    SetVarietyMovePayload payload = new()
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    };

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _varietyService.SetMoveAsync(_variety.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_variety.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when removing a variety move.")]
  public async Task Given_NotAllowed_When_RemoveMove_Then_PermissionDeniedException()
  {
    VarietyDto created = await _varietyService.SetMoveAsync(_variety.EntityId, new SetVarietyMovePayload
    {
      MoveId = _tackle.EntityId,
      LearningMethod = LearningMethod.LevelUp,
      Level = 1
    });
    VarietyMoveDto existing = Assert.Single(created.Moves);

    Context.User = new UserBuilder(Faker).Build();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _varietyService.RemoveMoveAsync(_variety.EntityId, existing.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_variety.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }
}
