using Bogus;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame;

public sealed class DomainCatalog
{
  public DomainCatalog(Faker? faker = null)
  {
    Faker = faker ?? new();
    World = new WorldBuilder(Faker).Build();
    Red = TrainerBuilder.Red(Faker, World);
    Blue = TrainerBuilder.Blue(Faker, World);
    MasterBall = ItemBuilder.MasterBall(Faker, World);
    PokeBall = new ItemBuilder(Faker)
      .WithWorld(World)
      .WithCategory(ItemCategory.PokeBall)
      .WithKey("poke-ball")
      .WithName("Poké Ball")
      .Build();
    Potion = ItemBuilder.Potion(Faker, World);
    Ability = AbilityBuilder.Overgrow(Faker, World);
    Species = SpeciesBuilder.Bulbasaur(Faker, World);
    Variety = VarietyBuilder.Bulbasaur(Faker, Species, World);
    Form = FormBuilder.Bulbasaur(Faker, Variety, Ability, World);
  }

  public Faker Faker { get; }
  public World World { get; }
  public Trainer Red { get; }
  public Trainer Blue { get; }
  public Item MasterBall { get; }
  public Item PokeBall { get; }
  public Item Potion { get; }
  public Ability Ability { get; }
  public PokemonSpecies Species { get; }
  public Variety Variety { get; }
  public Form Form { get; }
  public IPokemonRandomizer Randomizer { get; } = new DeterministicPokemonRandomizer();
  public Location PalletTown { get; } = new("Pallet Town");
  public Location CeruleanCity { get; } = new("Cerulean City");
  public Location PokemonCenter { get; } = new("Pokémon Center");

  public Specimen CreatePokemon(string? key = null, byte eggCycles = 0, AbilitySlot? abilitySlot = null, Gender? gender = null)
  {
    Key? pokemonKey = string.IsNullOrWhiteSpace(key) ? null : new Key(key);
    return new Specimen(Randomizer, PokemonId.NewId(World.Id), Species, Variety, Form, pokemonKey, gender, abilitySlot: abilitySlot, eggCycles: eggCycles);
  }

  public Specimen CreateOwnedPokemon(Trainer trainer, string? key = null, byte eggCycles = 0, Item? pokeBall = null)
  {
    Specimen pokemon = CreatePokemon(key, eggCycles);
    pokemon.Receive(trainer, pokeBall ?? MasterBall, PalletTown);
    return pokemon;
  }

  public Specimen CreateCaughtPokemon(Trainer trainer, string? key = null)
  {
    Specimen pokemon = CreatePokemon(key);
    pokemon.Catch(trainer, MasterBall, PalletTown);
    return pokemon;
  }

  public World OtherWorld() => new WorldBuilder(Faker).WithKey("other-world").Build();
}
