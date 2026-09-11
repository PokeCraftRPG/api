using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Caching;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Trainers;

[Trait(Traits.Category, Categories.Integration)]
public class TrainerIntegrationTests : IntegrationTests
{
  private readonly IAssetService _assetService;
  private readonly ICacheService _cacheService;
  private readonly ITrainerRepository _trainerRepository;
  private readonly ITrainerService _trainerService;
  private readonly IWorldRepository _worldRepository;

  private Trainer _trainer = null!;
  private TrainerDto _seeded = null!;

  public TrainerIntegrationTests()
  {
    _assetService = ServiceProvider.GetRequiredService<IAssetService>();
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _trainerService = ServiceProvider.GetRequiredService<ITrainerService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    _seeded = (await _trainerService.ReadAsync(_trainer.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new trainer.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    TrainerDto trainer = result.Trainer;
    Assert.NotNull(trainer);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, trainer.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, trainer.Id);
    }
    Assert.Equal(5, trainer.Version);
    Assert.Equal(Actor, trainer.CreatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(trainer.CreatedBy, trainer.UpdatedBy);
    Assert.True(trainer.CreatedOn < trainer.UpdatedOn);

    AssertMisty(payload, trainer);
  }

  [Fact(DisplayName = "It should create a trainer with a sprite and member.")]
  public async Task Given_SpriteAndMember_When_Create_Then_Created()
  {
    AssetDto sprite = await UploadSpriteAsync();
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    payload.SpriteId = sprite.Id;
    payload.MemberId = Context.User!.Id;

    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload);
    Assert.True(result.Created);
    TrainerDto trainer = result.Trainer;

    AssertMisty(payload, trainer);
    Assert.NotNull(trainer.Sprite);
    Assert.Equal(sprite.Id, trainer.Sprite.Id);
    Assert.Equal(Actor, trainer.Member);
  }

  [Fact(DisplayName = "It should read a trainer by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(_trainer.EntityId);
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should read a trainer by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should read a trainer by license.")]
  public async Task Given_License_When_Read_Then_Read()
  {
    TrainerDto? trainer = await _trainerService.ReadAsync(license: _seeded.License);
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should replace an existing trainer.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    payload.Key = _seeded.Key;
    Guid id = _trainer.EntityId;

    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    TrainerDto trainer = result.Trainer;
    Assert.NotNull(trainer);

    Assert.Equal(id, trainer.Id);
    Assert.Equal(8, trainer.Version);
    Assert.Equal(_seeded.CreatedBy, trainer.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, trainer.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, trainer.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertMisty(payload, trainer);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchTrainersPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<TrainerDto> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no trainer was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _trainerService.ReadAsync(_trainer.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many trainers were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    await _trainerRepository.SaveAsync(misty);

    TooManyResultsException<TrainerDto> exception = await Assert.ThrowsAsync<TooManyResultsException<TrainerDto>>(
      async () => await _trainerService.ReadAsync(_trainer.EntityId, misty.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the trainer was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _trainerService.UpdateAsync(Guid.Empty, new UpdateTrainerPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    Trainer brock = TrainerBuilder.Brock(Faker, Context.World);
    await _trainerRepository.SaveAsync([blue, misty, brock]);

    SearchTrainersPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("misty");
    payload.Search.Terms.Add("blue");
    payload.Ids.AddRange([blue.EntityId, misty.EntityId]);
    payload.Sort.Add(new SortOption<TrainerSort>(TrainerSort.Name, SortDirection.Descending));

    SearchResults<TrainerDto> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    TrainerDto trainer = Assert.Single(results.Items);
    Assert.Equal(blue.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should filter search results by gender.")]
  public async Task Given_Gender_When_Search_Then_Filtered()
  {
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    Trainer brock = TrainerBuilder.Brock(Faker, Context.World);
    await _trainerRepository.SaveAsync([misty, brock]);

    SearchTrainersPayload payload = new()
    {
      Gender = Gender.Female,
      Limit = 10
    };

    SearchResults<TrainerDto> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(1, results.Total);
    TrainerDto trainer = Assert.Single(results.Items);
    Assert.Equal(misty.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should filter search results by member.")]
  public async Task Given_Member_When_Search_Then_Filtered()
  {
    User member = await GrantMembershipAsync();
    SetupUsers(member);

    Trainer misty = new TrainerBuilder(Faker)
      .WithWorld(Context.World)
      .WithKey("misty")
      .WithName("Misty")
      .WithLicense("MISTY001")
      .WithGender(Gender.Female)
      .WithMember(new UserId(member.Id, _cacheService.Realm?.Id))
      .Build();
    await _trainerRepository.SaveAsync(misty);

    SearchTrainersPayload payload = new()
    {
      MemberId = member.Id,
      Limit = 10
    };

    SearchResults<TrainerDto> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(1, results.Total);
    TrainerDto trainer = Assert.Single(results.Items);
    Assert.Equal(misty.EntityId, trainer.Id);
    Assert.Equal(new Actor(member), trainer.Member);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a trainer and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Trainer.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a trainer and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    await _trainerRepository.SaveAsync(misty);

    CreateOrReplaceTrainerPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = misty.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Trainer.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a trainer and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    await _trainerRepository.SaveAsync(misty);

    UpdateTrainerPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = misty.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _trainerService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(id, exception.Data["EntityId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.Data["AttemptedKey"]);
    Assert.Equal(nameof(Trainer.Key), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw LicenseAlreadyUsedException when creating a trainer and the license conflicts.")]
  public async Task Given_LicenseConflict_When_Create_Then_LicenseAlreadyUsedException()
  {
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    payload.License = _seeded.License;
    Guid id = Guid.NewGuid();

    LicenseAlreadyUsedException exception = await Assert.ThrowsAsync<LicenseAlreadyUsedException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(id, exception.Data["TrainerId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(License.Format(payload.License!), exception.Data["AttemptedLicense"]);
    Assert.Equal(nameof(Trainer.License), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw LicenseAlreadyUsedException when updating a trainer and the license conflicts.")]
  public async Task Given_LicenseConflict_When_Update_Then_LicenseAlreadyUsedException()
  {
    Trainer misty = TrainerBuilder.Misty(Faker, Context.World);
    await _trainerRepository.SaveAsync(misty);

    UpdateTrainerPayload payload = new()
    {
      License = new Optional<string>(_seeded.License)
    };

    LicenseAlreadyUsedException exception = await Assert.ThrowsAsync<LicenseAlreadyUsedException>(
      async () => await _trainerService.UpdateAsync(misty.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(misty.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["ConflictId"]);
    Assert.Equal(_seeded.License, exception.Data["AttemptedLicense"]);
    Assert.Equal(nameof(Trainer.License), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
      Key = string.Empty,
      Money = -1
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _trainerService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateTrainerPayload payload = new()
    {
      Key = "not valid",
      Money = -1
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _trainerService.UpdateAsync(_trainer.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw InvalidAssetKindException when the sprite is not an image.")]
  public async Task Given_VideoSprite_When_Create_Then_InvalidAssetKindException()
  {
    AssetDto video = await UploadVideoAsync();
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    payload.SpriteId = video.Id;

    InvalidAssetKindException exception = await Assert.ThrowsAsync<InvalidAssetKindException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(video.Id, exception.Data["AssetId"]);
    Assert.Equal(AssetKind.Image, exception.Data["ExpectedKind"]);
    Assert.Equal(AssetKind.Video, exception.Data["AttemptedKind"]);
    Assert.Equal(nameof(Trainer.SpriteId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a trainer.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("CreateTrainer", exception.Data["Action"]);
    Assert.Null(exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a trainer.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload, _trainer.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(_trainer.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a trainer.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    UpdateTrainerPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _trainerService.UpdateAsync(_trainer.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(_trainer.GetEntity().ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw UserIsNotMemberException when the member is not in the world.")]
  public async Task Given_NotMember_When_Create_Then_UserIsNotMemberException()
  {
    User user = KrakenarFactory.Instance.NewUser(Faker);
    CreateOrReplaceTrainerPayload payload = CreateMistyPayload();
    payload.MemberId = user.Id;

    UserIsNotMemberException exception = await Assert.ThrowsAsync<UserIsNotMemberException>(
      async () => await _trainerService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(user.Id, exception.Data["UserId"]);
  }

  [Fact(DisplayName = "It should update an existing trainer.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _trainer.EntityId;
    CreateOrReplaceTrainerPayload create = CreateMistyPayload();
    UpdateTrainerPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      Gender = new Optional<Gender?>(create.Gender),
      Money = create.Money
    };

    TrainerDto? trainer = await _trainerService.UpdateAsync(id, payload);
    Assert.NotNull(trainer);

    Assert.Equal(id, trainer.Id);
    Assert.Equal(7, trainer.Version);
    Assert.Equal(_seeded.CreatedBy, trainer.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, trainer.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, trainer.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), trainer.Name);
    Assert.Equal(create.Summary?.Trim(), trainer.Summary);
    Assert.Equal(create.Content?.Trim(), trainer.Content);
    Assert.Equal(create.Gender, trainer.Gender);
    Assert.Equal(create.Money, trainer.Money);
  }

  private static CreateOrReplaceTrainerPayload CreateMistyPayload() => new()
  {
    Key = "misty",
    Name = " Misty ",
    Summary = "  The Cerulean City Gym Leader.  ",
    Content = "   A Water-type specialist.   ",
    License = "misty001",
    Gender = Gender.Female,
    Money = 3000
  };

  private static void AssertMisty(CreateOrReplaceTrainerPayload payload, TrainerDto trainer)
  {
    Assert.Equal(SlugHelper.Format(payload.Key), trainer.Key);
    Assert.Equal(payload.Name?.Trim(), trainer.Name);
    Assert.Equal(payload.Summary?.Trim(), trainer.Summary);
    Assert.Equal(payload.Content?.Trim(), trainer.Content);
    Assert.Equal(License.Format(payload.License!), trainer.License);
    Assert.Equal(payload.Gender, trainer.Gender);
    Assert.Equal(payload.Money, trainer.Money);
  }

  private async Task<AssetDto> UploadSpriteAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.jpg");
    Assert.True(File.Exists(path), $"Add a JPEG file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    return asset;
  }

  private async Task<AssetDto> UploadVideoAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.mp4");
    Assert.True(File.Exists(path), $"Add an MP4 file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    Assert.Equal(AssetKind.Video, asset.Kind);
    return asset;
  }

  private async Task<User> GrantMembershipAsync(params User[] members)
  {
    _cacheService.Realm = KrakenarFactory.Instance.Realm;

    if (members.Length == 0)
    {
      members = [KrakenarFactory.Instance.NewUser(Faker)];
    }

    foreach (User member in members)
    {
      Context.World!.GrantMembership(new UserId(member.Id, _cacheService.Realm!.Id), Context.ActorId);
    }
    await _worldRepository.SaveAsync(Context.World!);

    return members[0];
  }

  private void SetupUsers(params User[] users)
  {
    UserClient.Setup(x => x.SearchAsync(It.IsAny<SearchUsersPayload>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((SearchUsersPayload payload, CancellationToken _) =>
      {
        Dictionary<Guid, User> found = [];
        if (Context.User is not null && payload.Ids.Contains(Context.User.Id))
        {
          found[Context.User.Id] = Context.User;
        }
        foreach (User user in users)
        {
          if (payload.Ids.Contains(user.Id))
          {
            found[user.Id] = user;
          }
        }
        return new SearchResults<User>(found.Values);
      });
  }
}
