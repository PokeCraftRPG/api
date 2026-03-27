using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class TrainerIntegrationTests : IntegrationTests // TODO(fpion): tests have invalid display names, e.g. an form, or 'replace the existing'
{
  private readonly ITrainerRepository _trainerRepository;
  private readonly ITrainerService _trainerService;

  private Trainer _trainer = null!;

  public TrainerIntegrationTests() : base()
  {
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _trainerService = ServiceProvider.GetRequiredService<ITrainerService>();
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

  [Fact(DisplayName = "It should read an trainer by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _trainer.EntityId;
    TrainerModel? trainer = await _trainerService.ReadAsync(id);
    Assert.NotNull(trainer);
    Assert.Equal(id, trainer.Id);
  }

  [Fact(DisplayName = "It should read an trainer by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    TrainerModel? trainer = await _trainerService.ReadAsync(id: null, $" {_trainer.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(trainer);
    Assert.Equal(_trainer.EntityId, trainer.Id);
  }

  [Fact(DisplayName = "It should replace an existing trainer.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
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
    Assert.Equal(3, trainer.Version);
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
    Guid id = _trainer.EntityId;
    UpdateTrainerPayload payload = new()
    {
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
    Assert.Equal(2, trainer.Version);
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
  }
}
