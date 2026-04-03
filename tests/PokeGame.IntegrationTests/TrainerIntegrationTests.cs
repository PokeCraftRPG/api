using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Search;
using Krakenar.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Caching;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;
using PokeGame.Core.Worlds;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class TrainerIntegrationTests : IntegrationTests
{
  private readonly ICacheService _cacheService;
  private readonly ITrainerRepository _trainerRepository;
  private readonly ITrainerService _trainerService;
  private readonly IWorldRepository _worldRepository;

  private Trainer _trainer = null!;

  public TrainerIntegrationTests() : base()
  {
    _cacheService = ServiceProvider.GetRequiredService<ICacheService>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _trainerService = ServiceProvider.GetRequiredService<ITrainerService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = new TrainerBuilder(Faker).WithWorld(World).Build();
    await _trainerRepository.SaveAsync(_trainer);
  }

  [Theory(DisplayName = "It should create a new trainer.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceTrainerPayload payload = new()
    {
      License = "q-123456-3",
      Key = "ash-ketchum",
      Name = " Ash Ketchum ",
      Description = "  Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.  ",
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = "https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum",
      Notes = "   Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration.   "
    };

    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Trainer);

    TrainerModel trainer = result.Trainer;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, trainer.Id);
    }
    else
    {
      Assert.NotEqual(default, trainer.Id);
    }
    Assert.Equal(2, trainer.Version);
    Assert.Equal(Actor, trainer.CreatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, trainer.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.License.ToUpperInvariant(), trainer.License);
    Assert.Equal(payload.Key.ToLowerInvariant(), trainer.Key);
    Assert.Equal(payload.Name.Trim(), trainer.Name);
    Assert.Equal(payload.Description.Trim(), trainer.Description);
    Assert.Equal(payload.Gender, trainer.Gender);
    Assert.Equal(payload.Money, trainer.Money);
    Assert.Equal(payload.Sprite, trainer.Sprite);
    Assert.Equal(payload.Url, trainer.Url);
    Assert.Equal(payload.Notes.Trim(), trainer.Notes);
  }

  [Fact(DisplayName = "It should read a trainer by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _trainer.EntityId;
    TrainerModel? trainer = await _trainerService.ReadAsync(id);
    Assert.NotNull(trainer);
    Assert.Equal(id, trainer.Id);
  }

  [Fact(DisplayName = "It should read a trainer by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    TrainerModel? trainer = await _trainerService.ReadAsync(id: null, license: null, $" {_trainer.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should read a trainer by license.")]
  public async Task Given_License_When_Read_Then_Found()
  {
    TrainerModel? trainer = await _trainerService.ReadAsync(id: null, $" {_trainer.License.Value.ToUpperInvariant()} ");
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Trainer ashKetchum = TrainerBuilder.AshKetchum(Faker, World);
    Trainer brock = TrainerBuilder.Brock(Faker, World);
    Trainer may = TrainerBuilder.May(Faker, World);
    Trainer misty = TrainerBuilder.Misty(Faker, World);
    await _trainerRepository.SaveAsync([ashKetchum, brock, may, misty]);

    SearchTrainersPayload payload = new()
    {
      Ids = [ashKetchum.EntityId, brock.EntityId, Guid.Empty, misty.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("%m%"));
    payload.Sort.Add(new TrainerSortOption(TrainerSort.Key, isDescending: true));

    SearchResults<TrainerModel> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    TrainerModel trainer = Assert.Single(results.Items);
    Assert.Equal(ashKetchum.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Gender).")]
  public async Task Given_Gender_When_SearchAsync_Then_Results()
  {
    Trainer ashKetchum = TrainerBuilder.AshKetchum(Faker, World);
    Trainer may = TrainerBuilder.May(Faker, World);
    await _trainerRepository.SaveAsync([ashKetchum, may]);

    SearchTrainersPayload payload = new()
    {
      Ids = [ashKetchum.EntityId, may.EntityId],
      Gender = Faker.PickRandom<TrainerGender>()
    };

    SearchResults<TrainerModel> results = await _trainerService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    TrainerModel trainer = Assert.Single(results.Items);
    Assert.Equal((payload.Gender == TrainerGender.Male ? ashKetchum : may).EntityId, trainer.Id);
  }

  [Theory(DisplayName = "It should return the correct search results (OwnerId).")]
  [InlineData(" AnY  ")]
  [InlineData("none")]
  [InlineData("id")]
  public async Task Given_OwnerId_When_SearchAsync_Then_Results(string value)
  {
    User user = new UserBuilder().Build();
    _cacheService.SetActor(new Actor(user));

    Trainer ashKetchum = TrainerBuilder.AshKetchum(Faker, World);
    ashKetchum.SetOwnership(World.OwnerId, World.OwnerId);
    Trainer brock = TrainerBuilder.Brock(Faker, World);
    Trainer may = TrainerBuilder.May(Faker, World);
    may.SetOwnership(user.GetUserId(), World.OwnerId);
    Trainer misty = TrainerBuilder.Misty(Faker, World);
    await _trainerRepository.SaveAsync([ashKetchum, brock, may, misty]);

    SearchTrainersPayload payload = new()
    {
      Ids = [ashKetchum.EntityId, brock.EntityId, may.EntityId, misty.EntityId],
      OwnerId = value.Trim().Equals("id", StringComparison.InvariantCultureIgnoreCase) ? Actor.Id.ToString() : value
    };

    SearchResults<TrainerModel> results = await _trainerService.SearchAsync(payload);

    switch (value.Trim().ToLowerInvariant())
    {
      case "any":
        Assert.Equal(2, results.Total);
        Assert.Equal(results.Total, results.Items.Count);
        Assert.Contains(results.Items, trainer => trainer.Id == ashKetchum.EntityId);
        Assert.Contains(results.Items, trainer => trainer.Id == may.EntityId);
        break;
      case "none":
        Assert.Equal(2, results.Total);
        Assert.Equal(results.Total, results.Items.Count);
        Assert.Contains(results.Items, trainer => trainer.Id == brock.EntityId);
        Assert.Contains(results.Items, trainer => trainer.Id == misty.EntityId);
        break;
      default:
        Assert.Equal(1, results.Total);
        Assert.Equal(ashKetchum.EntityId, Assert.Single(results.Items).Id);
        break;
    }
  }

  [Fact(DisplayName = "It should replace an existing trainer.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    User member = new UserBuilder().Build();
    Actor actor = new(member);
    _cacheService.SetActor(actor);

    World.GrantMembership(member.GetUserId(), World.OwnerId);
    await _worldRepository.SaveAsync(World);

    CreateOrReplaceTrainerPayload payload = new()
    {
      OwnerId = member.Id,
      License = _trainer.License.Value.ToLowerInvariant(),
      Key = "ash-ketchum",
      Name = " Ash Ketchum ",
      Description = "  Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.  ",
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = "https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum",
      Notes = "   Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration.   "
    };
    Guid id = _trainer.EntityId;

    CreateOrReplaceTrainerResult result = await _trainerService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Trainer);

    TrainerModel trainer = result.Trainer;
    Assert.Equal(id, trainer.Id);
    Assert.Equal(4, trainer.Version);
    Assert.Equal(Actor, trainer.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.License.ToUpperInvariant(), trainer.License);
    Assert.Equal(payload.Key.ToLowerInvariant(), trainer.Key);
    Assert.Equal(payload.Name.Trim(), trainer.Name);
    Assert.Equal(payload.Description.Trim(), trainer.Description);
    Assert.Equal(payload.Gender, trainer.Gender);
    Assert.Equal(payload.Money, trainer.Money);
    Assert.Equal(payload.Sprite, trainer.Sprite);
    Assert.Equal(payload.Url, trainer.Url);
    Assert.Equal(payload.Notes.Trim(), trainer.Notes);
    Assert.Equal(actor, trainer.Owner);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
      License = Faker.TrainerLicense().Value,
      Key = _trainer.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _trainerService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Trainer", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_trainer.EntityId, exception.ConflictId);
    Assert.Equal(_trainer.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a license conflict.")]
  public async Task Given_LicenseConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
      License = _trainer.License.Value.ToLowerInvariant(),
      Key = "ash-ketchum"
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _trainerService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Trainer", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_trainer.EntityId, exception.ConflictId);
    Assert.Equal(_trainer.License.Value, exception.AttemptedValue);
    Assert.Equal("License", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing trainer.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    _trainer.SetOwnership(World.OwnerId, World.OwnerId);
    await _trainerRepository.SaveAsync(_trainer);

    _trainer = (await _trainerRepository.LoadAsync(_trainer.Id))!;
    Assert.NotNull(_trainer);
    Assert.Equal(World.OwnerId, _trainer.OwnerId);

    Guid id = _trainer.EntityId;
    UpdateTrainerPayload payload = new()
    {
      OwnerId = new Optional<Guid?>(null),
      Name = new Optional<string>(" Ash Ketchum "),
      Description = new Optional<string>("  Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.  "),
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum"),
      Notes = new Optional<string>("   Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration.   ")
    };

    TrainerModel? trainer = await _trainerService.UpdateAsync(id, payload);
    Assert.NotNull(trainer);

    Assert.Equal(id, trainer.Id);
    Assert.Equal(4, trainer.Version);
    Assert.Equal(Actor, trainer.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, trainer.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_trainer.License.Value, trainer.License);
    Assert.Equal(_trainer.Key.Value, trainer.Key);
    Assert.Equal(payload.Name.Value?.Trim(), trainer.Name);
    Assert.Equal(payload.Description.Value?.Trim(), trainer.Description);
    Assert.Equal(payload.Gender, trainer.Gender);
    Assert.Equal(payload.Money, trainer.Money);
    Assert.Equal(payload.Sprite.Value, trainer.Sprite);
    Assert.Equal(payload.Url.Value, trainer.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), trainer.Notes);
    Assert.Null(trainer.Owner);

    _trainer = (await _trainerRepository.LoadAsync(_trainer.Id))!;
    Assert.NotNull(_trainer);
    Assert.Null(_trainer.OwnerId);
  }
}
