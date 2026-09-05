using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IVarietyBuilder
{
  IVarietyBuilder WithId(VarietyId varietyId);
  IVarietyBuilder WithSpecies(PokemonSpecies species);
  IVarietyBuilder WithWorld(World? world);
  IVarietyBuilder WithKey(string key);
  IVarietyBuilder WithName(string? name);
  IVarietyBuilder WithSummary(string? summary);
  IVarietyBuilder WithContent(string? content);
  IVarietyBuilder WithIsDefault(bool isDefault);
  IVarietyBuilder WithCanChangeForm(bool canChangeForm);
  IVarietyBuilder WithGenderRatio(int? femaleRate);
  IVarietyBuilder WithGenus(string? genus);

  Variety Build();
}

public class VarietyBuilder : IVarietyBuilder
{
  private readonly Faker _faker;

  private bool _canChangeForm;
  private string? _content;
  private int? _genderRatio = 1;
  private string? _genus = "Seed";
  private bool _isDefault = true;
  private string _key = "bulbasaur";
  private string? _name = "Bulbasaur";
  private PokemonSpecies? _species;
  private string? _summary;
  private VarietyId? _varietyId;
  private World? _world;

  public VarietyBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IVarietyBuilder WithId(VarietyId varietyId)
  {
    _varietyId = varietyId;
    return this;
  }

  public IVarietyBuilder WithSpecies(PokemonSpecies species)
  {
    _species = species;
    return this;
  }

  public IVarietyBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IVarietyBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IVarietyBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IVarietyBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IVarietyBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public IVarietyBuilder WithIsDefault(bool isDefault)
  {
    _isDefault = isDefault;
    return this;
  }

  public IVarietyBuilder WithCanChangeForm(bool canChangeForm)
  {
    _canChangeForm = canChangeForm;
    return this;
  }

  public IVarietyBuilder WithGenderRatio(int? femaleRate)
  {
    _genderRatio = femaleRate;
    return this;
  }

  public IVarietyBuilder WithGenus(string? genus)
  {
    _genus = genus;
    return this;
  }

  public Variety Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    PokemonSpecies species = _species ?? SpeciesBuilder.Bulbasaur(_faker, world);
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Variety variety = _varietyId.HasValue
      ? new(_varietyId.Value, species.Id, key, actorId)
      : new(species, key, actorId);

    variety.SetDefault(_isDefault, actorId);
    variety.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);
    variety.SetTraits(_canChangeForm, GenderRatio.TryCreate(_genderRatio), Genus.TryCreate(_genus), actorId);

    return variety;
  }

  public static Variety Bulbasaur(Faker? faker = null, PokemonSpecies? species = null, World? world = null) => new VarietyBuilder(faker)
    .WithWorld(world)
    .WithSpecies(species ?? SpeciesBuilder.Bulbasaur(faker, world))
    .WithKey("bulbasaur")
    .WithName("Bulbasaur")
    .WithSummary("The default Bulbasaur form.")
    .WithContent("A Seed Pokémon with a plant bulb on its back.")
    .WithIsDefault(true)
    .WithCanChangeForm(false)
    .WithGenderRatio(1)
    .WithGenus("Seed")
    .Build();

  public static Variety Charmander(Faker? faker = null, PokemonSpecies? species = null, World? world = null) => new VarietyBuilder(faker)
    .WithWorld(world)
    .WithSpecies(species ?? SpeciesBuilder.Charmander(faker, world))
    .WithKey("charmander")
    .WithName("Charmander")
    .WithSummary("The default Charmander form.")
    .WithContent("A Lizard Pokémon that prefers hot things.")
    .WithIsDefault(true)
    .WithCanChangeForm(false)
    .WithGenderRatio(1)
    .WithGenus("Lizard")
    .Build();

  public static Variety Squirtle(Faker? faker = null, PokemonSpecies? species = null, World? world = null) => new VarietyBuilder(faker)
    .WithWorld(world)
    .WithSpecies(species ?? SpeciesBuilder.Squirtle(faker, world))
    .WithKey("squirtle")
    .WithName("Squirtle")
    .WithSummary("The default Squirtle form.")
    .WithContent("A Tiny Turtle Pokémon that squirts water.")
    .WithIsDefault(true)
    .WithCanChangeForm(false)
    .WithGenderRatio(1)
    .WithGenus("Turtle")
    .Build();

  public static Variety Pikachu(Faker? faker = null, PokemonSpecies? species = null, World? world = null) => new VarietyBuilder(faker)
    .WithWorld(world)
    .WithSpecies(species ?? SpeciesBuilder.Pikachu(faker, world))
    .WithKey("pikachu")
    .WithName("Pikachu")
    .WithSummary("The default Pikachu form.")
    .WithContent("A Mouse Pokémon that stores electricity in its cheeks.")
    .WithIsDefault(true)
    .WithCanChangeForm(true)
    .WithGenderRatio(4)
    .WithGenus("Mouse")
    .Build();
}
