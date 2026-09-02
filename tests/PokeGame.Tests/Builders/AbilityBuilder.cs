using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IAbilityBuilder
{
  IAbilityBuilder WithId(AbilityId abilityId);
  IAbilityBuilder WithWorld(World? world);
  IAbilityBuilder WithKey(string key);
  IAbilityBuilder WithName(string? name);
  IAbilityBuilder WithSummary(string? summary);
  IAbilityBuilder WithContent(string? content);

  Ability Build();
}

public class AbilityBuilder : IAbilityBuilder
{
  private readonly Faker _faker;

  private string? _content;
  private string _key = "overgrow";
  private string? _name = "Overgrow";
  private AbilityId? _abilityId;
  private string? _summary;
  private World? _world;

  public AbilityBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IAbilityBuilder WithId(AbilityId abilityId)
  {
    _abilityId = abilityId;
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

  public IAbilityBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IAbilityBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public Ability Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Ability ability = _abilityId.HasValue
      ? new(_abilityId.Value, key, actorId)
      : new(world, key, actorId);

    ability.Update(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);

    return ability;
  }

  public static Ability Overgrow(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("overgrow")
    .WithName("Overgrow")
    .WithSummary("Powers up Grass moves when HP is low.")
    .WithContent("When HP drops below one-third, Grass-type moves deal 50% more damage.")
    .Build();

  public static Ability Blaze(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("blaze")
    .WithName("Blaze")
    .WithSummary("Powers up Fire moves when HP is low.")
    .WithContent("When HP drops below one-third, Fire-type moves deal 50% more damage.")
    .Build();

  public static Ability Torrent(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("torrent")
    .WithName("Torrent")
    .WithSummary("Powers up Water moves when HP is low.")
    .WithContent("When HP drops below one-third, Water-type moves deal 50% more damage.")
    .Build();

  public static Ability Static(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey("static")
    .WithName("Static")
    .WithSummary("May paralyze on contact.")
    .WithContent("Contact with the Pokémon may cause paralysis.")
    .Build();
}
