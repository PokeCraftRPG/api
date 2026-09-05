using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IMoveBuilder
{
  IMoveBuilder WithId(MoveId moveId);
  IMoveBuilder WithWorld(World? world);
  IMoveBuilder WithType(PokemonType type);
  IMoveBuilder WithCategory(MoveCategory category);
  IMoveBuilder WithKey(string key);
  IMoveBuilder WithName(string? name);
  IMoveBuilder WithSummary(string? summary);
  IMoveBuilder WithContent(string? content);
  IMoveBuilder WithAccuracy(int? accuracy);
  IMoveBuilder WithPower(int? power);
  IMoveBuilder WithPowerPoints(int? powerPoints);

  Move Build();
}

public class MoveBuilder : IMoveBuilder
{
  private readonly Faker _faker;

  private int? _accuracy = 100;
  private MoveCategory _category = MoveCategory.Physical;
  private string? _content;
  private string _key = "tackle";
  private MoveId? _moveId;
  private string? _name = "Tackle";
  private int? _power = 40;
  private int? _powerPoints = 35;
  private string? _summary;
  private PokemonType _type = PokemonType.Normal;
  private World? _world;

  public MoveBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IMoveBuilder WithId(MoveId moveId)
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

  public IMoveBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IMoveBuilder WithContent(string? content)
  {
    _content = content;
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

  public IMoveBuilder WithPowerPoints(int? powerPoints)
  {
    _powerPoints = powerPoints;
    return this;
  }

  public Move Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Move move = _moveId.HasValue
      ? new(_moveId.Value, _type, _category, key, actorId)
      : new(world, _type, _category, key, actorId);

    move.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);
    move.SetMechanics(Accuracy.TryCreate(_accuracy), Power.TryCreate(_power), PowerPoints.TryCreate(_powerPoints), actorId);

    return move;
  }

  public static Move Tackle(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Physical)
    .WithKey("tackle")
    .WithName("Tackle")
    .WithSummary("A physical attack.")
    .WithContent("The target is physically slammed with a full-body tackle.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(35)
    .Build();

  public static Move Ember(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Fire)
    .WithCategory(MoveCategory.Special)
    .WithKey("ember")
    .WithName("Ember")
    .WithSummary("A weak Fire attack.")
    .WithContent("The target is attacked with small flames. May inflict a burn.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(25)
    .Build();

  public static Move WaterGun(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Water)
    .WithCategory(MoveCategory.Special)
    .WithKey("water-gun")
    .WithName("Water Gun")
    .WithSummary("A weak Water attack.")
    .WithContent("The target is blasted with a forceful shot of water.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(25)
    .Build();

  public static Move ThunderShock(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Electric)
    .WithCategory(MoveCategory.Special)
    .WithKey("thunder-shock")
    .WithName("Thunder Shock")
    .WithSummary("A weak Electric attack.")
    .WithContent("The target is jolted with a weak electric shock. May cause paralysis.")
    .WithAccuracy(100)
    .WithPower(40)
    .WithPowerPoints(30)
    .Build();
}
