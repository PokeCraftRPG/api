using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Varieties;

namespace PokeGame;

public sealed class DeterministicPokemonRandomizer : IPokemonRandomizer
{
  public AbilitySlot AbilitySlot() => Core.Abilities.AbilitySlot.Primary;
  public PokemonCharacteristic Characteristic(IIndividualValues individualValues) => PokemonCharacteristics.Pick(individualValues, new Random(0));
  public Gender Gender(GenderRatio ratio) => Core.Gender.Male;
  public IndividualValues IndividualValues() => new(10, 11, 12, 13, 14, 15);
  public PokemonNature Nature() => PokemonNatures.Hardy;
  public bool Shininess() => false;
  public PokemonSize Size() => new(128);
  public PokemonType TeraType(IFormTypes types) => types.Primary;
}
