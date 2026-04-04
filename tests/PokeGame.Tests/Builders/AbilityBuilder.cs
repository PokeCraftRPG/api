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
    Slug key = _key ?? new("an-ability");

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

  public static Ability Blaze(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("blaze"))
    .WithName(new Name("Blaze"))
    .WithDescription(new Description("Powers up Fire-type moves when the Pokémon's HP is low."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Blaze_(Ability)"))
    .WithNotes(new Notes("Boosts Fire-type moves when HP is low (≈1/3 or less), increasing damage by ~50%. Common Ability of Fire starters; no effect outside battle."))
    .Build();

  public static Ability LightningRod(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("lightning-rod"))
    .WithName(new Name("Lightning Rod"))
    .WithDescription(new Description("The Pokémon draws in all Electric-type moves. Instead of taking damage from them, its Sp. Atk stat is boosted."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Lightning_Rod_(Ability)"))
    .WithNotes(new Notes("Draws all Electric-type moves to itself, nullifies their damage, and boosts Sp. Atk."))
    .Build();

  public static Ability SandVeil(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("sand-veil"))
    .WithName(new Name("Sand Veil"))
    .WithDescription(new Description("Boosts the Pokémon's evasiveness in a sandstorm."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Sand_Veil_(Ability)"))
    .WithNotes(new Notes("Boosts evasion in sandstorms and prevents sandstorm damage; reduces wild encounters in earlier gens, but has no effect outside battle in newer games."))
    .Build();

  public static Ability Static(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("static"))
    .WithName(new Name("Static"))
    .WithDescription(new Description("The Pokémon is charged with static electricity and may paralyze attackers that make direct contact with it."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Static_(Ability)"))
    .WithNotes(new Notes("On contact, 30% chance to paralyze the attacker (each hit can trigger). Outside battle, increases chance of encountering Electric-type Pokémon."))
    .Build();

  public static Ability SurgeSurfer(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("surge-surfer"))
    .WithName(new Name("Surge Surfer"))
    .WithDescription(new Description("Doubles the Pokémon's Speed stat on Electric Terrain."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Surge_Surfer_(Ability)"))
    .WithNotes(new Notes("On Electric Terrain, this Pokémon’s Speed is doubled; no effect outside battle."))
    .Build();

  public static Ability ThickFat(Faker? faker = null, World? world = null) => new AbilityBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("thick-fat"))
    .WithName(new Name("Thick Fat"))
    .WithDescription(new Description("The Pokémon is protected by a layer of thick fat, which halves the damage taken from Fire- and Ice-type moves."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Thick_Fat_(Ability)"))
    .WithNotes(new Notes("Halves damage taken from Fire- and Ice-type moves by reducing incoming power during calculation. No effect outside battle."))
    .Build();
}
