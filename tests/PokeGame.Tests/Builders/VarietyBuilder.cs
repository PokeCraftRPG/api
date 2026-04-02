using Bogus;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IVarietyBuilder
{
  IVarietyBuilder WithId(VarietyId? id);
  IVarietyBuilder WithWorld(World? world);
  IVarietyBuilder WithSpecies(SpeciesAggregate? species);
  IVarietyBuilder IsDefault(bool isDefault = true);
  IVarietyBuilder WithKey(Slug? key);
  IVarietyBuilder WithName(Name? name);
  IVarietyBuilder WithGenus(Genus? genus);
  IVarietyBuilder WithDescription(Description? description);
  IVarietyBuilder WithGenderRatio(GenderRatio? genderRatio);
  IVarietyBuilder CanChangeForm(bool canChangeForm = true);
  IVarietyBuilder WithUrl(Url? url);
  IVarietyBuilder WithNotes(Notes? notes);
  IVarietyBuilder WithEvolutionMove(Move move);
  IVarietyBuilder WithEvolutionMove(MoveId moveId);
  IVarietyBuilder WithLevelMove(Move move, Level level);
  IVarietyBuilder WithLevelMove(MoveId moveId, Level level);
  IVarietyBuilder ClearChanges(bool clearChanges = true);

  Variety Build();
}

public class VarietyBuilder : IVarietyBuilder
{
  private readonly Faker _faker;
  private readonly Dictionary<MoveId, Level?> _moves = [];

  private bool _canChangeForm = false;
  private bool _clearChanges = false;
  private Description? _description = null;
  private GenderRatio? _genderRatio = null;
  private Genus? _genus = null;
  private VarietyId? _id = null;
  private bool _isDefault = false;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private SpeciesAggregate? _species = null;
  private Url? _url = null;
  private World? _world = null;

  public VarietyBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IVarietyBuilder WithId(VarietyId? id)
  {
    _id = id;
    return this;
  }

  public IVarietyBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IVarietyBuilder WithSpecies(SpeciesAggregate? species)
  {
    _species = species;
    return this;
  }

  public IVarietyBuilder IsDefault(bool isDefault = true)
  {
    _isDefault = isDefault;
    return this;
  }

  public IVarietyBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IVarietyBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IVarietyBuilder WithGenus(Genus? genus)
  {
    _genus = genus;
    return this;
  }

  public IVarietyBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IVarietyBuilder WithGenderRatio(GenderRatio? genderRatio)
  {
    _genderRatio = genderRatio;
    return this;
  }

  public IVarietyBuilder CanChangeForm(bool canChangeForm = true)
  {
    _canChangeForm = canChangeForm;
    return this;
  }

  public IVarietyBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IVarietyBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IVarietyBuilder WithEvolutionMove(Move move) => WithEvolutionMove(move.Id);
  public IVarietyBuilder WithEvolutionMove(MoveId moveId)
  {
    _moves[moveId] = null;
    return this;
  }

  public IVarietyBuilder WithLevelMove(Move move, Level level) => WithLevelMove(move.Id, level);
  public IVarietyBuilder WithLevelMove(MoveId moveId, Level level)
  {
    _moves[moveId] = level;
    return this;
  }

  public IVarietyBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Variety Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    SpeciesAggregate species = _species ?? new SpeciesBuilder(_faker).WithWorld(world).Build();
    Slug key = _key ?? new("a-variety");

    Variety variety = _id.HasValue ? new(species, _isDefault, key, world.OwnerId, _id.Value) : new(world, species, _isDefault, key);

    variety.Name = _name;
    variety.Genus = _genus;
    variety.Description = _description;

    variety.GenderRatio = _genderRatio;

    variety.CanChangeForm = _canChangeForm;

    variety.Url = _url;
    variety.Notes = _notes;

    variety.Update(world.OwnerId);

    foreach (KeyValuePair<MoveId, Level?> move in _moves)
    {
      if (move.Value is null)
      {
        variety.SetEvolutionMove(move.Key, world.OwnerId);
      }
      else
      {
        variety.SetLevelMove(move.Key, move.Value, world.OwnerId);
      }
    }

    if (_clearChanges)
    {
      variety.ClearChanges();
    }

    return variety;
  }

  public static Variety Eevee(Faker? faker = null, World? world = null, SpeciesAggregate? species = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new VarietyBuilder(faker)
      .WithWorld(world)
      .WithSpecies(species ?? SpeciesBuilder.Eevee(faker, world))
      .IsDefault()
      .WithKey(new Slug("eevee"))
      .WithName(new Name("Eevee"))
      .WithGenus(new Genus("Evolution"))
      .WithDescription(new Description("Thanks to its unstable genetic makeup, this special Pokémon conceals many different possible evolutions."))
      .WithGenderRatio(new GenderRatio(7))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Eevee_(Pok%C3%A9mon)#Trivia"))
      .WithNotes(new Notes("This is the default variety."))
      .Build();
  }

  public static Variety Pichu(Faker? faker = null, World? world = null, SpeciesAggregate? species = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new VarietyBuilder(faker)
      .WithWorld(world)
      .WithSpecies(species ?? SpeciesBuilder.Pichu(faker, world))
      .IsDefault()
      .WithKey(new Slug("pichu"))
      .WithName(new Name("Pichu"))
      .WithGenus(new Genus("Tiny Mouse"))
      .WithDescription(new Description("It is not yet skilled at storing electricity. It may send out a jolt if amused or startled."))
      .WithGenderRatio(new GenderRatio(4))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pichu_(Pok%C3%A9mon)"))
      .WithNotes(new Notes("This is the default variety."))
      .Build();
  }

  public static Variety Pikachu(Faker? faker = null, World? world = null, SpeciesAggregate? species = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new VarietyBuilder(faker)
      .WithWorld(world)
      .WithSpecies(species ?? SpeciesBuilder.Pikachu(faker, world))
      .IsDefault()
      .WithKey(new Slug("pikachu"))
      .WithName(new Name("Pikachu"))
      .WithGenus(new Genus("Mouse"))
      .WithDescription(new Description("It has small electric sacs on both its cheeks. When in a tough spot, this Pokémon discharges electricity."))
      .WithGenderRatio(new GenderRatio(4))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"))
      .WithNotes(new Notes("This is the default variety."))
      .Build();
  }

  public static Variety Raichu(Faker? faker = null, World? world = null, SpeciesAggregate? species = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new VarietyBuilder(faker)
      .WithWorld(world)
      .WithSpecies(species ?? SpeciesBuilder.Raichu(faker, world))
      .IsDefault()
      .WithKey(new Slug("raichu"))
      .WithName(new Name("Raichu"))
      .WithGenus(new Genus("Mouse"))
      .WithDescription(new Description("When its electricity builds, its muscles are stimulated, and it becomes more aggressive than usual."))
      .WithGenderRatio(new GenderRatio(4))
      .CanChangeForm()
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"))
      .WithNotes(new Notes("This is the default variety."))
      .Build();
  }
}
