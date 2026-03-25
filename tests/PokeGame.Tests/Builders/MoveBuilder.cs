using Bogus;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IMoveBuilder
{
  IMoveBuilder WithId(MoveId? id);
  IMoveBuilder WithWorld(World? world);
  IMoveBuilder WithType(PokemonType? type);
  IMoveBuilder WithCategory(MoveCategory? category);
  IMoveBuilder WithKey(Slug? key);
  IMoveBuilder WithName(Name? name);
  IMoveBuilder WithDescription(Description? description);
  IMoveBuilder WithAccuracy(Accuracy? accuracy);
  IMoveBuilder WithPower(Power? power);
  IMoveBuilder WithPowerPoints(PowerPoints? powerPoints);
  IMoveBuilder WithUrl(Url? url);
  IMoveBuilder WithNotes(Notes? notes);
  IMoveBuilder ClearChanges(bool clearChanges = true);

  Move Build();
}

public class MoveBuilder : IMoveBuilder
{
  private readonly Faker _faker;

  private Accuracy? _accuracy = null;
  private MoveCategory? _category = null;
  private Description? _description = null;
  private MoveId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Power? _power = null;
  private PowerPoints? _powerPoints = null;
  private PokemonType? _type = null;
  private Url? _url = null;
  private World? _world = null;
  private bool _clearChanges = false;

  public MoveBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IMoveBuilder WithId(MoveId? id)
  {
    _id = id;
    return this;
  }

  public IMoveBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IMoveBuilder WithType(PokemonType? type)
  {
    _type = type;
    return this;
  }

  public IMoveBuilder WithCategory(MoveCategory? category)
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

  public IMoveBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IMoveBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IMoveBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Move Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    PokemonType type = _type ?? PokemonType.Normal;
    MoveCategory category = _category ?? MoveCategory.Status;
    Slug key = _key ?? new("a-move");
    PowerPoints powerPoints = _powerPoints ?? new(PowerPoints.MinimumValue);

    Move move = _id.HasValue ? new(type, category, key, powerPoints, world.OwnerId, _id.Value) : new(world, type, category, key, powerPoints);
    move.Name = _name;
    move.Description = _description;
    move.Accuracy = _accuracy;
    move.Power = _power;
    move.Url = _url;
    move.Notes = _notes;
    move.Update(world.OwnerId);

    if (_clearChanges)
    {
      move.ClearChanges();
    }

    return move;
  }

  public static Move Agility(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Psychic)
    .WithCategory(MoveCategory.Status)
    .WithKey(new Slug("agility"))
    .WithName(new Name("Agility"))
    .WithDescription(new Description("The user relaxes and lightens its body to move faster. This sharply boosts its Speed stat."))
    .WithPowerPoints(new PowerPoints(30))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Agility_(move)"))
    .WithNotes(new Notes("Psychic status move that sharply raises the user’s Speed (+2 stages); no damage, ideal for acting first and setting up sweeps."))
    .Build();

  public static Move QuickAttack(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Normal)
    .WithCategory(MoveCategory.Physical)
    .WithKey(new Slug("quick-attack"))
    .WithName(new Name("Quick Attack"))
    .WithDescription(new Description("The user lunges at the target to inflict damage, moving at blinding speed. This move always goes first."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(40))
    .WithPowerPoints(new PowerPoints(30))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Quick_Attack_(move)"))
    .WithNotes(new Notes("Fast physical Normal move that always goes first (+1 priority), dealing 40 damage with perfect accuracy; simple, reliable early strike."))
    .Build();

  public static Move SweetKiss(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Fairy)
    .WithCategory(MoveCategory.Status)
    .WithKey(new Slug("sweet-kiss"))
    .WithName(new Name("Sweet Kiss"))
    .WithDescription(new Description("The user kisses the target with a sweet, angelic cuteness that causes confusion."))
    .WithAccuracy(new Accuracy(75))
    .WithPowerPoints(new PowerPoints(10))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Sweet_Kiss_(move)"))
    .WithNotes(new Notes("Fairy status move that confuses the target (75% accuracy); no damage, useful for disruption and control. \r\n"))
    .Build();

  public static Move ThunderPunch(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Electric)
    .WithCategory(MoveCategory.Physical)
    .WithKey(new Slug("thunder-punch"))
    .WithName(new Name("Thunder Punch"))
    .WithDescription(new Description("The target is attacked with an electrified punch. This may also leave the target with paralysis."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(75))
    .WithPowerPoints(new PowerPoints(15))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Thunder_Punch_(move)"))
    .WithNotes(new Notes("Electric physical attack (75 power, 100% accuracy) hitting adjacent targets; has ~10% chance to paralyze, with some variation across games."))
    .Build();

  public static Move ThunderShock(Faker? faker = null, World? world = null) => new MoveBuilder(faker)
    .WithWorld(world)
    .WithType(PokemonType.Electric)
    .WithCategory(MoveCategory.Special)
    .WithKey(new Slug("thunder-shock"))
    .WithName(new Name("Thunder Shock"))
    .WithDescription(new Description("The user attacks the target with a jolt of electricity. This may also leave the target with paralysis."))
    .WithAccuracy(new Accuracy(100))
    .WithPower(new Power(40))
    .WithPowerPoints(new PowerPoints(30))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)"))
    .WithNotes(new Notes("Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed."))
    .Build();
}
