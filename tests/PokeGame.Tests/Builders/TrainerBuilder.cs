using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Identity;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ITrainerBuilder
{
  ITrainerBuilder WithId(TrainerId trainerId);
  ITrainerBuilder WithWorld(World? world);
  ITrainerBuilder WithKey(string key);
  ITrainerBuilder WithName(string? name);
  ITrainerBuilder WithSummary(string? summary);
  ITrainerBuilder WithContent(string? content);
  ITrainerBuilder WithLicense(string? license);
  ITrainerBuilder WithGender(Gender? gender);
  ITrainerBuilder WithMoney(int money);
  ITrainerBuilder WithSprite(Asset? sprite);
  ITrainerBuilder WithMember(UserId? memberId);

  Trainer Build();
}

public class TrainerBuilder : ITrainerBuilder
{
  private readonly Faker _faker;

  private string? _content;
  private Gender? _gender;
  private string _key = "red";
  private string? _license = "RED001";
  private UserId? _memberId;
  private int _money;
  private string? _name = "Red";
  private Asset? _sprite;
  private string? _summary;
  private TrainerId? _trainerId;
  private World? _world;

  public TrainerBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ITrainerBuilder WithId(TrainerId trainerId)
  {
    _trainerId = trainerId;
    return this;
  }

  public ITrainerBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public ITrainerBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public ITrainerBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public ITrainerBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public ITrainerBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public ITrainerBuilder WithLicense(string? license)
  {
    _license = license;
    return this;
  }

  public ITrainerBuilder WithGender(Gender? gender)
  {
    _gender = gender;
    return this;
  }

  public ITrainerBuilder WithMoney(int money)
  {
    _money = money;
    return this;
  }

  public ITrainerBuilder WithSprite(Asset? sprite)
  {
    _sprite = sprite;
    return this;
  }

  public ITrainerBuilder WithMember(UserId? memberId)
  {
    _memberId = memberId;
    return this;
  }

  public Trainer Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Trainer trainer = _trainerId.HasValue
      ? new(_trainerId.Value, key, actorId)
      : new(world, key, actorId);

    trainer.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);
    trainer.SetLicense(License.TryCreate(_license), actorId);
    trainer.SetGender(_gender, actorId);
    trainer.SetMoney(new Money(_money), actorId);
    trainer.SetSprite(_sprite, actorId);
    trainer.SetMember(_memberId, actorId);

    return trainer;
  }

  public static Trainer Red(Faker? faker = null, World? world = null) => new TrainerBuilder(faker)
    .WithWorld(world)
    .WithKey("red")
    .WithName("Red")
    .WithSummary("The rival from Pallet Town.")
    .WithContent("A determined trainer from Kanto.")
    .WithLicense("RED001")
    .WithGender(Gender.Male)
    .Build();

  public static Trainer Blue(Faker? faker = null, World? world = null) => new TrainerBuilder(faker)
    .WithWorld(world)
    .WithKey("blue")
    .WithName("Blue")
    .WithSummary("The grandson of Professor Oak.")
    .WithContent("A confident trainer from Kanto.")
    .WithLicense("BLUE001")
    .WithGender(Gender.Male)
    .Build();

  public static Trainer Misty(Faker? faker = null, World? world = null) => new TrainerBuilder(faker)
    .WithWorld(world)
    .WithKey("misty")
    .WithName("Misty")
    .WithSummary("The Cerulean City Gym Leader.")
    .WithContent("A Water-type specialist.")
    .WithLicense("MISTY001")
    .WithGender(Gender.Female)
    .Build();

  public static Trainer Brock(Faker? faker = null, World? world = null) => new TrainerBuilder(faker)
    .WithWorld(world)
    .WithKey("brock")
    .WithName("Brock")
    .WithSummary("The Pewter City Gym Leader.")
    .WithContent("A Rock-type specialist.")
    .WithLicense("BROCK001")
    .WithGender(Gender.Male)
    .Build();
}
