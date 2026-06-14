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

  public static Ability Blaze(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("blaze"))
    .WithName(new Name("Blaze"))
    .WithDescription(new Description("Powers up Fire-type moves when the Pokémon's HP is low."))
    .Build();
  public static Ability LightningRod(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("lightning-rod"))
    .WithName(new Name("Lightning Rod"))
    .WithDescription(new Description("The Pokémon draws in all Electric-type moves. Instead of taking damage from them, its Sp. Atk stat is boosted."))
    .Build();
  public static Ability Overgrow(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("overgrow"))
    .WithName(new Name("Overgrow"))
    .WithDescription(new Description("Powers up Grass-type moves when the Pokémon's HP is low."))
    .Build();
  public static Ability Static(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("static"))
    .WithName(new Name("Static"))
    .WithDescription(new Description("The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it."))
    .Build();
  public static Ability Torrent(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("torrent"))
    .WithName(new Name("Torrent"))
    .WithDescription(new Description("Powers up Water-type moves when the Pokémon's HP is low"))
    .Build();
}
