using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IAbilityBuilder
{
  IAbilityBuilder WithId(AbilityId? abilityId);
  IAbilityBuilder WithWorld(World? world);
  IAbilityBuilder WithKey(Slug? key);
  IAbilityBuilder WithName(Name? name);
  IAbilityBuilder WithDescription(Description? description);

  Ability Build();
}

public class AbilityBuilder : IAbilityBuilder
{
  private readonly Faker _faker = new();

  private Description? _description = null;
  private Slug? _key = null;
  private Name? _name = null;
  private AbilityId? _abilityId = null;
  private World? _world = null;

  public AbilityBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IAbilityBuilder WithId(AbilityId? abilityId)
  {
    _abilityId = abilityId;
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

  public Ability Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("Ability");
    ActorId actorId = world.OwnerId.ActorId;

    Ability ability = _abilityId.HasValue ? new(_abilityId.Value, key, actorId) : new(world, key, actorId);
    ability.Rename(_name, actorId);
    ability.Describe(_description, actorId);

    return ability;
  }
}
