using Bogus;
using PokeGame.Core.Abilities;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IAbilityBuilder
{
  IAbilityBuilder WithId(Guid id);
  IAbilityBuilder WithWorld(World? world);
  IAbilityBuilder WithKey(string key);
  IAbilityBuilder WithName(string? name);
  IAbilityBuilder WithDescription(string? description);

  Ability Build();
}

public class AbilityBuilder : IAbilityBuilder
{
  private readonly Faker _faker;

  private string? _description = null;
  private Guid? _id = null;
  private string _key = "ability";
  private string? _name = null;
  private World? _world = null;

  public AbilityBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IAbilityBuilder WithId(Guid id)
  {
    _id = id;
    return this;
  }

  public IAbilityBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IAbilityBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IAbilityBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IAbilityBuilder WithDescription(string? description)
  {
    _description = description;
    return this;
  }

  public Ability Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    return new Ability(world, _key, world.OwnerId, _id, _name, _description);
  }

  public static Ability Adaptability(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("adaptability")
    .WithName("Adaptability")
    .WithDescription("Powers up moves of the same type as the Pokémon.")
    .Build();
  public static Ability LightningRod(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("lightning-rod")
    .WithName("Lightning Rod")
    .WithDescription("The Pokémon draws in all Electric-type moves. Instead of taking damage from them, its Sp. Atk stat is boosted.")
    .Build();
  public static Ability Static(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("static")
    .WithName("Static")
    .WithDescription("The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it.")
    .Build();
  public static Ability SurgeSurfer(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("surge-surfer")
    .WithName("Surge Surfer")
    .WithDescription("Doubles the Pokémon's Speed stat on Electric Terrain.")
    .Build();
}
