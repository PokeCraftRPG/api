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
    License license = _license ?? new("Q-123456-3");
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
}
