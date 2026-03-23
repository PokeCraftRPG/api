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
}
