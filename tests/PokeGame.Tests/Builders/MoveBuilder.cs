using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IMoveBuilder
{
  IMoveBuilder WithId(MoveId? moveId);
  IMoveBuilder WithWorld(World? world);
  IMoveBuilder WithType(PokemonType type);
  IMoveBuilder WithCategory(MoveCategory category);
  IMoveBuilder WithKey(Slug? key);
  IMoveBuilder WithName(Name? name);
  IMoveBuilder WithDescription(Description? description);
  IMoveBuilder WithAccuracy(Accuracy? accuracy);
  IMoveBuilder WithPower(Power? power);
  IMoveBuilder WithPowerPoints(PowerPoints? powerPoints);

  Move Build();
}

public class MoveBuilder : IMoveBuilder
{
  private readonly Faker _faker = new();

  private Accuracy? _accuracy = null;
  private MoveCategory _category = MoveCategory.Physical;
  private Description? _description = null;
  private Slug? _key = null;
  private MoveId? _moveId = null;
  private Name? _name = null;
  private Power? _power = null;
  private PowerPoints? _powerPoints = null;
  private PokemonType _type = PokemonType.Normal;
  private World? _world = null;

  public MoveBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IMoveBuilder WithId(MoveId? moveId)
  {
    _moveId = moveId;
    return this;
  }

  public IMoveBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IMoveBuilder WithType(PokemonType type)
  {
    _type = type;
    return this;
  }

  public IMoveBuilder WithCategory(MoveCategory category)
  {
    _category = category;
    return this;
  }

  public IMoveBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IMoveBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IMoveBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IMoveBuilder WithAccuracy(Accuracy? accuracy)
  {
    _accuracy = accuracy;
    return this;
  }

  public IMoveBuilder WithPower(Power? power)
  {
    _power = power;
    return this;
  }

  public IMoveBuilder WithPowerPoints(PowerPoints? powerPoints)
  {
    _powerPoints = powerPoints;
    return this;
  }

  public Move Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("move");
    PowerPoints powerPoints = _powerPoints ?? new(1);
    ActorId actorId = world.OwnerId.ActorId;

    Move move = _moveId.HasValue
      ? new(_moveId.Value, _type, _category, key, powerPoints, _accuracy, _power, actorId)
      : new(world, _type, _category, key, powerPoints, _accuracy, _power, actorId);
    move.Rename(_name, actorId);
    move.Describe(_description, actorId);
    move.SetGameData(_accuracy, _power, _powerPoints ?? new PowerPoints(35), actorId);

    return move;
  }

  public static Move BodySlam(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Physical)
    .WithKey(new Slug("body-slam"))
    .WithName(new Name("Body Slam"))
    .WithDescription(new Description("A reckless, full-body charge attack for slamming into the target. This may also leave the target with paralysis."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(85))
    .WithPowerPoints(new PowerPoints(15))
    .Build();
  public static Move Flamethrower(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Fire)
    .WithCategory(MoveCategory.Special)
    .WithKey(new Slug("flamethrower"))
    .WithName(new Name("Flamethrower"))
    .WithDescription(new Description("The target is scorched with an intense blast of fire. This may also leave the target with a burn."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(90))
    .WithPowerPoints(new PowerPoints(15))
    .Build();
  public static Move Tackle(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Physical)
    .WithKey(new Slug("tackle"))
    .WithName(new Name("Tackle"))
    .WithDescription(new Description("A physical attack in which the user charges and slams into the target with its whole body."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(40))
    .WithPowerPoints(new PowerPoints(35))
    .Build();
  public static Move Thunderbolt(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Electric)
    .WithCategory(MoveCategory.Special)
    .WithKey(new Slug("thunderbolt"))
    .WithName(new Name("Thunderbolt"))
    .WithDescription(new Description("A strong electric blast crashes down on the target. This may also leave the target with paralysis."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(90))
    .WithPowerPoints(new PowerPoints(15))
    .Build();
}
