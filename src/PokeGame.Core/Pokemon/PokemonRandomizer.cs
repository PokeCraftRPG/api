using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

public interface IPokemonRandomizer
{
  AbilitySlot AbilitySlot(FormAbilities abilities);
  PokemonCharacteristic Characteristic(IIndividualValues individualValues);
  Gender Gender(GenderRatio ratio);
  IndividualValues IndividualValues();
  PokemonNature Nature();
  bool Shininess();
  PokemonSize Size();
  PokemonType TeraType(IFormTypes types);
}

internal class PokemonRandomizer : IPokemonRandomizer
{
  private readonly Random _random = new();

  public AbilitySlot AbilitySlot(FormAbilities abilities)
  {
    return abilities.SecondaryId.HasValue && _random.Next(2) == 1 ? Abilities.AbilitySlot.Secondary : Abilities.AbilitySlot.Primary;
  }

  public PokemonCharacteristic Characteristic(IIndividualValues individualValues) => PokemonCharacteristics.Pick(individualValues, _random);

  public Gender Gender(GenderRatio ratio)
  {
    int random = _random.Next(GenderRatio.FemaleRatio);
    return random < GenderRatio.FemaleRatio ? Core.Gender.Female : Core.Gender.Male;
  }

  public IndividualValues IndividualValues() => new(
    (byte)_random.Next(32),
    (byte)_random.Next(32),
    (byte)_random.Next(32),
    (byte)_random.Next(32),
    (byte)_random.Next(32),
    (byte)_random.Next(32));

  public PokemonNature Nature()
  {
    PokemonNature[] natures = PokemonNatures.All().ToArray();
    int index = _random.Next(natures.Length);
    return natures[index];
  }

  public bool Shininess() => _random.Next(100) == 0;

  public PokemonSize Size()
  {
    int random = _random.Next(byte.MaxValue + 1);
    return new PokemonSize((byte)random);
  }

  public PokemonType TeraType(IFormTypes types) => types.Secondary.HasValue && _random.Next(2) == 1 ? types.Secondary.Value : types.Primary;
}
