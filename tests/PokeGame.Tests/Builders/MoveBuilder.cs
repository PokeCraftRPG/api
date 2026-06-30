using Bogus;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IMoveBuilder
{
  IMoveBuilder WithId(Guid id);
  IMoveBuilder WithWorld(World? world);
  IMoveBuilder WithType(PokemonType type);
  IMoveBuilder WithCategory(MoveCategory category);
  IMoveBuilder WithKey(string key);
  IMoveBuilder WithName(string? name);
  IMoveBuilder WithDescription(string? description);
  IMoveBuilder WithAccuracy(int? accuracy);
  IMoveBuilder WithPower(int? power);
  IMoveBuilder WithPowerPoints(int powerPoints);

  Move Build();
}

public class MoveBuilder : IMoveBuilder
{
  private readonly Faker _faker;

  private int? _accuracy = null;
  private MoveCategory _category = MoveCategory.Status;
  private string? _description = null;
  private Guid? _id = null;
  private string _key = "move";
  private string? _name = null;
  private int? _power = null;
  private int _powerPoints = 1;
  private PokemonType _type = PokemonType.Normal;
  private World? _world = null;

  public MoveBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IMoveBuilder WithId(Guid id)
  {
    _id = id;
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

  public IMoveBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IMoveBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IMoveBuilder WithDescription(string? description)
  {
    _description = description;
    return this;
  }

  public IMoveBuilder WithAccuracy(int? accuracy)
  {
    _accuracy = accuracy;
    return this;
  }

  public IMoveBuilder WithPower(int? power)
  {
    _power = power;
    return this;
  }

  public IMoveBuilder WithPowerPoints(int powerPoints)
  {
    _powerPoints = powerPoints;
    return this;
  }

  public Move Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    return new Move(world, _type, _category, _key, _powerPoints, world.OwnerId, _id, _name, _description, _accuracy, _power);
  }

  public static Move QuickAttack(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Physical)
    .WithKey("quick-attack")
    .WithName("Quick Attack")
    .WithDescription("The user lunges at the target to inflict damage, moving at blinding speed. This move always goes first.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(30)
    .Build();
  public static Move SweetKiss(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Fairy)
    .WithCategory(MoveCategory.Status)
    .WithKey("sweet-kiss")
    .WithName("Sweet Kiss")
    .WithDescription("The user kisses the target with a sweet, angelic cuteness that causes confusion.")
    .WithAccuracy(75)
    .WithPowerPoints(10)
    .Build();
  public static Move TailWhip(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Status)
    .WithKey("tail-whip")
    .WithName("Tail Whip")
    .WithDescription("The user wags its tail cutely, making opposing Pokémon less wary. This lowers their Defense stats.")
    .WithAccuracy(100)
    .WithPowerPoints(30)
    .Build();
  public static Move ThunderShock(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Electric)
    .WithCategory(MoveCategory.Special)
    .WithKey("thunder-shock")
    .WithName("Thunder Shock")
    .WithDescription("The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(30)
    .Build();
}
