using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Rosters;

[Trait(Traits.Category, Categories.Integration)]
public class TagIntegrationTests : IntegrationTests
{
  private readonly IRosterService _rosterService;
  private readonly ITrainerRepository _trainerRepository;

  private Trainer _trainer = null!;

  public TagIntegrationTests()
  {
    _rosterService = ServiceProvider.GetRequiredService<IRosterService>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);
  }

  [Theory(DisplayName = "It should create a new tag.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceTagPayload payload = CreateFavoritesPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceTagResult result = await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, payload, id);
    Assert.True(result.Created);
    TagDto tag = result.Tag;

    if (id.HasValue)
    {
      Assert.Equal(id.Value, tag.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, tag.Id);
    }
    Assert.Equal(Actor, tag.CreatedBy);
    Assert.Equal(DateTime.UtcNow, tag.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(tag.CreatedBy, tag.UpdatedBy);
    Assert.Equal(tag.CreatedOn, tag.UpdatedOn, TimeSpan.FromMilliseconds(1));
    AssertTag(payload, tag);

    TagDto? read = await _rosterService.ReadTagAsync(_trainer.EntityId, tag.Id);
    Assert.NotNull(read);
    Assert.Equal(tag.Id, read.Id);
    AssertTag(payload, read);
  }

  [Fact(DisplayName = "It should replace an existing tag.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceTagResult created = await CreateAsync(CreateFavoritesPayload());
    CreateOrReplaceTagPayload payload = new()
    {
      Name = "Competitive",
      Color = new ColorDto(0, 128, 255)
    };

    CreateOrReplaceTagResult result = await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, payload, created.Tag.Id);
    Assert.False(result.Created);
    TagDto tag = result.Tag;

    Assert.Equal(created.Tag.Id, tag.Id);
    Assert.Equal(created.Tag.CreatedBy, tag.CreatedBy);
    Assert.Equal(created.Tag.CreatedOn, tag.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, tag.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, tag.UpdatedOn, TimeSpan.FromSeconds(10));
    AssertTag(payload, tag);
  }

  [Fact(DisplayName = "It should update a tag partially.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    CreateOrReplaceTagResult created = await CreateAsync(CreateFavoritesPayload());

    UpdateTagPayload payload = new()
    {
      Name = "Breeding",
      Color = new Optional<ColorDto>(null)
    };

    TagDto? tag = await _rosterService.UpdateTagAsync(_trainer.EntityId, created.Tag.Id, payload);
    Assert.NotNull(tag);
    Assert.Equal(created.Tag.Id, tag.Id);
    Assert.Equal("Breeding", tag.Name);
    Assert.Null(tag.Color);
    Assert.Equal(created.Tag.CreatedBy, tag.CreatedBy);
    Assert.Equal(Actor, tag.UpdatedBy);

    TagDto? read = await _rosterService.ReadTagAsync(_trainer.EntityId, created.Tag.Id);
    Assert.NotNull(read);
    Assert.Equal("Breeding", read.Name);
    Assert.Null(read.Color);
  }

  [Fact(DisplayName = "It should delete a tag.")]
  public async Task Given_Exists_When_Delete_Then_Deleted()
  {
    CreateOrReplaceTagResult created = await CreateAsync(CreateFavoritesPayload());

    TagDto? deleted = await _rosterService.DeleteTagAsync(_trainer.EntityId, created.Tag.Id);
    Assert.NotNull(deleted);
    Assert.Equal(created.Tag.Id, deleted.Id);
    Assert.Equal(created.Tag.Name, deleted.Name);

    Assert.Null(await _rosterService.ReadTagAsync(_trainer.EntityId, created.Tag.Id));
  }

  [Fact(DisplayName = "It should return null when reading a missing tag.")]
  public async Task Given_Missing_When_Read_Then_Null()
  {
    Assert.Null(await _rosterService.ReadTagAsync(_trainer.EntityId, Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should return null when updating a missing tag.")]
  public async Task Given_Missing_When_Update_Then_Null()
  {
    Assert.Null(await _rosterService.UpdateTagAsync(_trainer.EntityId, Guid.NewGuid(), new UpdateTagPayload { Name = "Missing" }));
  }

  [Fact(DisplayName = "It should return null when deleting a missing tag.")]
  public async Task Given_Missing_When_Delete_Then_Null()
  {
    Assert.Null(await _rosterService.DeleteTagAsync(_trainer.EntityId, Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the trainer does not exist.")]
  public async Task Given_MissingTrainer_When_CreateOrReplace_Then_EntityNotFoundException()
  {
    Guid missingTrainerId = Guid.NewGuid();

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _rosterService.CreateOrReplaceTagAsync(missingTrainerId, CreateFavoritesPayload()));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingTrainerId, exception.Data["EntityId"]);
    Assert.Equal("TrainerId", exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_CreateOrReplace_Then_InvalidCommandException()
  {
    CreateOrReplaceTagPayload payload = new()
    {
      Name = string.Empty
    };

    await Assert.ThrowsAsync<InvalidCommandException>(
      async () => await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when managing tags.")]
  public async Task Given_NotAllowed_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, CreateFavoritesPayload()));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("ManageTags", exception.Data["Action"]);
    Assert.Equal(new Roster(_trainer.Id).GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  private async Task<CreateOrReplaceTagResult> CreateAsync(CreateOrReplaceTagPayload payload)
    => await _rosterService.CreateOrReplaceTagAsync(_trainer.EntityId, payload);

  private static CreateOrReplaceTagPayload CreateFavoritesPayload() => new()
  {
    Name = "Favorites",
    Color = new ColorDto(255, 64, 0)
  };

  private static void AssertTag(CreateOrReplaceTagPayload payload, TagDto tag)
  {
    Assert.Equal(payload.Name, tag.Name);
    if (payload.Color is null)
    {
      Assert.Null(tag.Color);
    }
    else
    {
      Assert.NotNull(tag.Color);
      Assert.Equal(payload.Color.Red, tag.Color.Red);
      Assert.Equal(payload.Color.Green, tag.Color.Green);
      Assert.Equal(payload.Color.Blue, tag.Color.Blue);
    }
  }
}
