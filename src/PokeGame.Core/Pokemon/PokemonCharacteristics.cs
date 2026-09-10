namespace PokeGame.Core.Pokemon;

public static class PokemonCharacteristics
{
  private static readonly Dictionary<PokemonStatistic, PokemonCharacteristic[]> _texts = new(capacity: 6)
  {
    [PokemonStatistic.HP] = [PokemonCharacteristic.LovesToEat, PokemonCharacteristic.TakesPlentyOfSiestas, PokemonCharacteristic.NodsOffALot, PokemonCharacteristic.ScattersThingsOften, PokemonCharacteristic.LikesToRelax],
    [PokemonStatistic.Attack] = [PokemonCharacteristic.ProudOfItsPower, PokemonCharacteristic.LikesToThrashAbout, PokemonCharacteristic.ALittleQuickTempered, PokemonCharacteristic.LikesToFight, PokemonCharacteristic.QuickTempered],
    [PokemonStatistic.Defense] = [PokemonCharacteristic.SturdyBody, PokemonCharacteristic.CapableOfTakingHits, PokemonCharacteristic.HighlyPersistent, PokemonCharacteristic.GoodEndurance, PokemonCharacteristic.GoodPerseverance],
    [PokemonStatistic.SpecialAttack] = [PokemonCharacteristic.HighlyCurious, PokemonCharacteristic.Mischievous, PokemonCharacteristic.ThoroughlyCunning, PokemonCharacteristic.OftenLostInThought, PokemonCharacteristic.VeryFinicky],
    [PokemonStatistic.SpecialDefense] = [PokemonCharacteristic.StrongWilled, PokemonCharacteristic.SomewhatVain, PokemonCharacteristic.StronglyDefiant, PokemonCharacteristic.HatesToLose, PokemonCharacteristic.SomewhatStubborn],
    [PokemonStatistic.Speed] = [PokemonCharacteristic.LikesToRun, PokemonCharacteristic.AlertToSounds, PokemonCharacteristic.ImpetuousAndSilly, PokemonCharacteristic.SomewhatOfAClown, PokemonCharacteristic.QuickToFlee]
  };

  public static PokemonCharacteristic Pick(IIndividualValues individualValues, Random random)
  {
    byte maximum = FindMaximum(individualValues);
    PokemonStatistic statistic = GetStatistic(individualValues, maximum, random);
    return _texts[statistic][maximum % 5];
  }
  private static byte FindMaximum(IIndividualValues individualValues)
  {
    byte[] values = [individualValues.HP, individualValues.Attack, individualValues.Defense, individualValues.SpecialAttack, individualValues.SpecialDefense, individualValues.Speed];
    return values.Max();
  }
  private static PokemonStatistic GetStatistic(IIndividualValues individualValues, byte maximum, Random random)
  {
    List<PokemonStatistic> statistics = new(capacity: 6);
    if (individualValues.HP == maximum)
    {
      statistics.Add(PokemonStatistic.HP);
    }
    if (individualValues.Attack == maximum)
    {
      statistics.Add(PokemonStatistic.Attack);
    }
    if (individualValues.Defense == maximum)
    {
      statistics.Add(PokemonStatistic.Defense);
    }
    if (individualValues.SpecialAttack == maximum)
    {
      statistics.Add(PokemonStatistic.SpecialAttack);
    }
    if (individualValues.SpecialDefense == maximum)
    {
      statistics.Add(PokemonStatistic.SpecialDefense);
    }
    if (individualValues.Speed == maximum)
    {
      statistics.Add(PokemonStatistic.Speed);
    }
    int index = random.Next(statistics.Count);
    return statistics[index];
  }
}
