using Bogus;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IEvolutionBuilder
{
  IEvolutionBuilder WithId(EvolutionId? id);
  IEvolutionBuilder WithWorld(World? world);
  IEvolutionBuilder WithSource(Form? source);
  IEvolutionBuilder WithTarget(Form? target);
  IEvolutionBuilder WithTrigger(EvolutionTrigger? trigger);
  IEvolutionBuilder OnLevelUp();
  IEvolutionBuilder OnItem(Item? item);
  IEvolutionBuilder OnTrade();
  IEvolutionBuilder WithItem(Item? item);
  IEvolutionBuilder WithLevel(Level? level);
  IEvolutionBuilder IsFriendship(bool isFriendship = true);
  IEvolutionBuilder WithGender(PokemonGender? gender);
  IEvolutionBuilder WithHeldItem(Item? heldItem);
  IEvolutionBuilder WithKnownMove(Move? knownMove);
  IEvolutionBuilder WithLocation(Location? location);
  IEvolutionBuilder WithTimeOfDay(TimeOfDay? timeOfDay);
  IEvolutionBuilder ClearChanges(bool clearChanges = true);

  Evolution Build();
}

public class EvolutionBuilder : IEvolutionBuilder
{
  private readonly Faker _faker;

  private bool _clearChanges = false;
  private PokemonGender? _gender = null;
  private Item? _heldItem = null;
  private EvolutionId? _id = null;
  private bool _isFriendship = false;
  private Item? _item = null;
  private Move? _knownMove = null;
  private Level? _level = null;
  private Location? _location = null;
  private Form? _source = null;
  private Form? _target = null;
  private TimeOfDay? _timeOfDay = null;
  private EvolutionTrigger? _trigger = null;
  private World? _world = null;

  public EvolutionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IEvolutionBuilder WithId(EvolutionId? id)
  {
    _id = id;
    return this;
  }

  public IEvolutionBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IEvolutionBuilder WithSource(Form? source)
  {
    _source = source;
    return this;
  }

  public IEvolutionBuilder WithTarget(Form? target)
  {
    _target = target;
    return this;
  }

  public IEvolutionBuilder WithTrigger(EvolutionTrigger? trigger)
  {
    _trigger = trigger;
    return this;
  }

  public IEvolutionBuilder OnLevelUp()
  {
    _trigger = EvolutionTrigger.Level;
    return this;
  }

  public IEvolutionBuilder OnItem(Item? item)
  {
    _trigger = EvolutionTrigger.Item;
    _item = item;
    return this;
  }

  public IEvolutionBuilder OnTrade()
  {
    _trigger = EvolutionTrigger.Trade;
    return this;
  }

  public IEvolutionBuilder WithItem(Item? item)
  {
    _item = item;
    return this;
  }

  public IEvolutionBuilder WithLevel(Level? level)
  {
    _level = level;
    return this;
  }

  public IEvolutionBuilder IsFriendship(bool isFriendship = true)
  {
    _isFriendship = isFriendship;
    return this;
  }

  public IEvolutionBuilder WithGender(PokemonGender? gender)
  {
    _gender = gender;
    return this;
  }

  public IEvolutionBuilder WithHeldItem(Item? heldItem)
  {
    _heldItem = heldItem;
    return this;
  }

  public IEvolutionBuilder WithKnownMove(Move? knownMove)
  {
    _knownMove = knownMove;
    return this;
  }

  public IEvolutionBuilder WithLocation(Location? location)
  {
    _location = location;
    return this;
  }

  public IEvolutionBuilder WithTimeOfDay(TimeOfDay? timeOfDay)
  {
    _timeOfDay = timeOfDay;
    return this;
  }

  public IEvolutionBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Evolution Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Form source = _source ?? FormBuilder.Pikachu(_faker, world);
    Form target = _target ?? FormBuilder.Raichu(_faker, world);
    EvolutionTrigger trigger = _trigger ?? EvolutionTrigger.Item;
    Item? item = trigger == EvolutionTrigger.Item ? (_item ?? ItemBuilder.ThunderStone(_faker, world)) : null;

    Evolution evolution = _id.HasValue ? new(source, target, trigger, item, world.OwnerId, _id.Value) : new(world, source, target, trigger, item);

    evolution.Level = _level;
    evolution.Friendship = _isFriendship;
    evolution.Gender = _gender;
    evolution.HeldItemId = _heldItem?.Id;
    evolution.KnownMoveId = _knownMove?.Id;
    evolution.Location = _location;
    evolution.TimeOfDay = _timeOfDay;
    evolution.Update(world.OwnerId);

    if (_clearChanges)
    {
      evolution.ClearChanges();
    }

    return evolution;
  }

  public static Evolution PikachuToRaichu(Faker? faker = null, World? world = null, Form? source = null, Form? target = null, Item? item = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new EvolutionBuilder(faker)
      .WithWorld(world)
      .WithSource(source ?? FormBuilder.Pikachu(faker, world))
      .WithTarget(target ?? FormBuilder.Raichu(faker, world))
      .OnItem(item ?? ItemBuilder.ThunderStone(faker, world))
      .Build();
  }
}
