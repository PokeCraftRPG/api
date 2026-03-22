using Bogus;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IAbilityBuilder
{
  IAbilityBuilder WithId(AbilityId? id);
  IAbilityBuilder WithWorld(World? world);
  IAbilityBuilder WithKey(Slug? key);
  IAbilityBuilder WithName(Name? name);
  IAbilityBuilder WithDescription(Description? description);
  IAbilityBuilder WithUrl(Url? url);
  IAbilityBuilder WithNotes(Notes? notes);
  IAbilityBuilder ClearChanges(bool clearChanges = true);

  Ability Build();
}

public class AbilityBuilder : IAbilityBuilder
{
  private readonly Faker _faker;

  private Description? _description = null;
  private AbilityId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Url? _url = null;
  private World? _world = null;
  private bool _clearChanges = false;

  public AbilityBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IAbilityBuilder WithId(AbilityId? id)
  {
    _id = id;
    return this;
  }

  public IAbilityBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IAbilityBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IAbilityBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IAbilityBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IAbilityBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IAbilityBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IAbilityBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Ability Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("overgrow");

    Ability ability = _id.HasValue ? new(key, world.OwnerId, _id.Value) : new(world, key);
    ability.Name = _name;
    ability.Description = _description;
    ability.Url = _url;
    ability.Notes = _notes;
    ability.Update(world.OwnerId);

    if (_clearChanges)
    {
      ability.ClearChanges();
    }

    return ability;
  }
}
