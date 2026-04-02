using Bogus;
using PokeGame.Core;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ITrainerBuilder
{
  ITrainerBuilder WithId(TrainerId? id);
  ITrainerBuilder WithWorld(World? world);
  ITrainerBuilder WithLicense(License? license);
  ITrainerBuilder WithKey(Slug? key);
  ITrainerBuilder WithName(Name? name);
  ITrainerBuilder WithDescription(Description? description);
  ITrainerBuilder WithGender(TrainerGender? gender);
  ITrainerBuilder WithMoney(Money? money);
  ITrainerBuilder WithSprite(Url? sprite);
  ITrainerBuilder WithUrl(Url? url);
  ITrainerBuilder WithNotes(Notes? notes);
  ITrainerBuilder ClearChanges(bool clearChanges = true);

  Trainer Build();
}

public class TrainerBuilder : ITrainerBuilder
{
  private readonly Faker _faker;

  private Description? _description = null;
  private TrainerGender? _gender = null;
  private TrainerId? _id = null;
  private Slug? _key = null;
  private License? _license = null;
  private Money? _money = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Url? _sprite = null;
  private Url? _url = null;
  private World? _world = null;
  private bool _clearChanges = false;

  public TrainerBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public ITrainerBuilder WithId(TrainerId? id)
  {
    _id = id;
    return this;
  }

  public ITrainerBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public ITrainerBuilder WithLicense(License? license)
  {
    _license = license;
    return this;
  }

  public ITrainerBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public ITrainerBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public ITrainerBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public ITrainerBuilder WithGender(TrainerGender? gender)
  {
    _gender = gender;
    return this;
  }

  public ITrainerBuilder WithMoney(Money? money)
  {
    _money = money;
    return this;
  }

  public ITrainerBuilder WithSprite(Url? sprite)
  {
    _sprite = sprite;
    return this;
  }

  public ITrainerBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public ITrainerBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public ITrainerBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Trainer Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    License license = _license ?? _faker.TrainerLicense();
    Slug key = _key ?? new("a-trainer");
    TrainerGender gender = _gender ?? _faker.PickRandom<TrainerGender>();

    Trainer trainer = _id.HasValue ? new(license, key, gender, world.OwnerId, _id.Value) : new(world, license, key, gender);
    trainer.Name = _name;
    trainer.Description = _description;
    trainer.Money = _money ?? trainer.Money;
    trainer.Sprite = _sprite;
    trainer.Url = _url;
    trainer.Notes = _notes;
    trainer.Update(world.OwnerId);

    if (_clearChanges)
    {
      trainer.ClearChanges();
    }

    return trainer;
  }

  public static Trainer AshKetchum(Faker? faker = null, World? world = null)
  {
    faker ??= new();
    return new TrainerBuilder(faker)
      .WithWorld(world)
      .WithLicense(faker.TrainerLicense())
      .WithKey(new Slug("ash-ketchum"))
      .WithName(new Name("Ash Ketchum"))
      .WithDescription(new Description("Ash Ketchum is a 10-year-old Trainer from Pallet Town, known for his bond with Pikachu and his journey across regions, mastering multiple battle styles."))
      .WithGender(TrainerGender.Male)
      .WithMoney(new Money(faker.Random.Int(0, 999999)))
      .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/c/cd/Ash_JN.png"))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum"))
      .WithNotes(new Notes("Ash is a versatile Champion-level Trainer using Z-Moves, Mega Evolution, and Dynamax; notable for broad type coverage and long winning streaks."))
      .Build();
  }

  public static Trainer Brock(Faker? faker = null, World? world = null)
  {
    faker ??= new();
    return new TrainerBuilder(faker)
      .WithWorld(world)
      .WithLicense(faker.TrainerLicense())
      .WithKey(new Slug("brock"))
      .WithName(new Name("Brock"))
      .WithDescription(new Description("Brock is a loyal companion of Ash, known for his early catches, varied team, and long presence across the series, with strong ties to family and Pokémon care."))
      .WithGender(TrainerGender.Male)
      .WithMoney(new Money(faker.Random.Int(0, 999999)))
      .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/a/a0/Brock_Journeys.png"))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Brock_(anime)"))
      .WithNotes(new Notes("Brock is a long-running support character with diverse evolution methods, early captures, and even Mega Evolution use; useful as a mentor or multi-role NPC."))
      .Build();
  }

  public static Trainer May(Faker? faker = null, World? world = null)
  {
    faker ??= new();
    return new TrainerBuilder(faker)
      .WithWorld(world)
      .WithLicense(faker.TrainerLicense())
      .WithKey(new Slug("may"))
      .WithName(new Name("May"))
      .WithDescription(new Description("May is a Contest-focused Trainer and companion of Ash, known for her Pokédex, starter Pokémon, and journey across Hoenn and beyond."))
      .WithGender(TrainerGender.Female)
      .WithMoney(new Money(faker.Random.Int(0, 999999)))
      .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/3/36/May_AG.png"))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/May_(anime)"))
      .WithNotes(new Notes("May is a Contest specialist with cross-region experience, unique milestones, and ties to Professor Oak—useful as a rival or coordinator NPC."))
      .Build();
  }

  public static Trainer Misty(Faker? faker = null, World? world = null)
  {
    faker ??= new();
    return new TrainerBuilder(faker)
      .WithWorld(world)
      .WithLicense(faker.TrainerLicense())
      .WithKey(new Slug("misty"))
      .WithName(new Name("Misty"))
      .WithDescription(new Description("Misty is a Water-type specialist and early companion of Ash, known for her strong team, leadership at Cerulean Gym, and recurring appearances."))
      .WithGender(TrainerGender.Female)
      .WithMoney(new Money(faker.Random.Int(0, 999999)))
      .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/9/99/Misty_Journeys.png"))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Misty_(anime)"))
      .WithNotes(new Notes("Misty is a veteran Water-type Trainer with a large roster, early Gym experience, and repeated returns—ideal as a rival, mentor, or Gym Leader NPC."))
      .Build();
  }
}
